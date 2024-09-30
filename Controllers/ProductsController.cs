using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ST10251759_CLDV6212_POE_Part_1.Models;
using ST10251759_CLDV6212_POE_Part_1.Services;
using System.Text;

/*

 ==============================Code Attribution==================================

 Author: w3schools
 Link: https://www.w3schools.com/html/
 Date Accessed: 16 August 2024

 Author: HTML Codex
 Link:https://htmlcodex.com/
 Date Accessed: 16 August 2024

 Author: w3schools
 Link: https://www.w3schools.com/css/
 Date Accessed: 16 August 2024

 Author: w3schools
 Link: https://www.w3schools.com/js/
 Date Accessed: 16 August 2024

 Author: Mick Gouweloos
 Link: https://github.com/mickymouse777/Cloud_Storage/tree/master/Cloud_Storage
 Date Accessed: 16 August 2024

 *********All Images used throughout project are adapted from Pinterest (https://www.furiousmotorsport.com/)*************

 ==============================Code Attribution==================================
     
 */

namespace ST10251759_CLDV6212_POE_Part_1.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly BlobService _blobService;
        private readonly TableStorageService _tableStorageService;
        private readonly QueueService _queueService;
        private readonly HttpClient _httpClient;


        public ProductsController(BlobService blobService, TableStorageService tableStorageService, QueueService queueService, HttpClient httpClient)
        {
            _blobService = blobService;
            _tableStorageService = tableStorageService;
            _queueService = queueService;
            _httpClient = httpClient;
        }

        [HttpGet]
        public async Task<IActionResult> AddProductAsync()
        {
            // PASSING USER LOGGED IN
            if (User.Identity.Name != null)
            {
                ViewBag.IsLoggedIn = true;
                var userName = User.Identity.Name;
                ViewBag.UserName = userName;

            }
            else
            {
                ViewBag.UserRole = "Guest";
                ViewBag.IsLoggedIn = false;
                return RedirectToAction("NeedToLogin", "Error");
            }

            // Get the current logged -in user's email or username from Identity
            var userEmail = User.Identity?.Name;


            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized();
            }

            // Fetch the user from Azure Table Storage based on their email (RowKey)
            var user = await _tableStorageService.GetUserByUsernameAsync(userEmail);

            if (user == null)
            {
                return RedirectToAction("NeedToLogin", "Error");
            }

            // Check the role of the user
            if (user.Role == "Admin")
            {
                //Pass variable to view for display nar bar links
                ViewBag.UserRole = "Admin";
                return View();
            }
            else if (user.Role == "Customer")
            {
                //Return customer narbar links
                ViewBag.UserRole = "Customer";
                return RedirectToAction("AccessDenied", "Error");
            }
            else
            {
                //return links for guests
                ViewBag.UserRole = "Guest";
                return RedirectToAction("NeedToLogin", "Error");
            }

            
        }

        //Method to Add Products using a Blob Function and a Table Storage Function
        [HttpPost]
        public async Task<IActionResult> AddProduct(Product product, IFormFile file)
        {
            // Check if a file has been uploaded by the user.
            if (file != null)
            {
                // Retrieve the name of the file from the uploaded file.
                var fileName = Path.GetFileName(file.FileName);

                // Open a stream to read the contents of the uploaded file asynchronously.
                using var stream = file.OpenReadStream();

                // Upload the file to Azure Blob Storage using the Blob service.
                await _blobService.UploadAsync(stream, fileName);

                // Construct the URL for the uploaded image and assign it to the product's ImageUrlPath property.
                // Ensure that the URL follows the correct format for accessing the blob storage.
                product.ImageUrlPath = "https://st10251759cldv6212poe.blob.core.windows.net/products/" + fileName; // Ensure the URL is correct
            }

            // Check if the ModelState is valid, which means all validation checks for the Product model passed.
            if (ModelState.IsValid)
            {
                // Retrieve all existing products from the table storage asynchronously.
                var products = await _tableStorageService.GetAllProductsAsync();

                // Determine the next available ProductId by finding the maximum existing ProductId and adding 1.
                // If no products exist, start with ProductId = 1.
                int nextProductId = products.Any() ? products.Max(p => p.ProductId) + 1 : 1;

                // Determine the next RowKey by finding the maximum existing RowKey and adding 1.
                // If no products exist, start with RowKey = 1.
                int nextRowKey = products.Any() ? products.Max(p => int.Parse(p.RowKey)) + 1 : 1;

                // Assign the PartitionKey for the product. If the Category is null, default to "DefaultCategory".
                product.PartitionKey = product.Category ?? "DefaultCategory";

                // Assign the generated RowKey to the product.
                product.RowKey = nextRowKey.ToString();

                // Assign the generated ProductId to the product.
                product.ProductId = nextProductId; // Assign the generated ProductId

                // Serialize the product details into JSON format for sending to the external function.
                var json = JsonConvert.SerializeObject
                (
                    new Product
                    {
                        ProductId = nextProductId, // Include the new ProductId
                        PartitionKey = product.Category ?? "DefaultCategory", // Include the determined PartitionKey
                        RowKey = nextRowKey.ToString(), // Include the new RowKey
                        ProductDescription = product.ProductDescription, // Include the description of the product
                        Name = product.Name, // Include the product's name
                        Price = product.Price, // Include the product's price
                        Category = product.Category, // Include the product's category
                        ImageUrlPath = product.ImageUrlPath, // Include the URL for the product's image
                        Timestamp = product.Timestamp, // Include the timestamp for the product
                    }
                );

                // Create an HTTP content object containing the serialized JSON.
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Send a POST request to the specified Azure function URL to add the product.
                var response = await _httpClient.PostAsync("https://addproductfunction.azurewebsites.net/api/AddProductFunction?code=KKSh_41Kw0qJEIGdkSvUz_Lt_AvyaRpFiX_HTuFZQZdoAzFudl_giQ%3D%3D", content);

                // Check if the response from the Azure function indicates a successful operation.
                if (!response.IsSuccessStatusCode)
                {
                    // If not successful, return a BadRequest response with an error message.
                    return BadRequest("Failed to add product.");
                }

                // Log a message indicating the successful upload of the product image and its corresponding ID.
                string imageUploadMessage = $"Product ID:{product.ProductId}, Image uploaded successfully.";

                // Send a message to the message queue indicating the image upload status.
                await _queueService.SendMessageAsync("imageupload", imageUploadMessage);

                // Redirect the user to the Index action upon successful addition of the product.
                return RedirectToAction("Index");
            }

            // If ModelState is not valid, return the current view with the product model for user corrections.
            return View(product);
        }


        [HttpPost]
        public async Task<IActionResult> DeleteProduct(string partitionKey, string rowKey, Product product)
        {
            if (product != null && !string.IsNullOrEmpty(product.ImageUrlPath))
            {
                // Delete the associated blob image by calling the Azure Function through BlobService
                await _blobService.DeleteBlobAsync(product.ImageUrlPath);
            }

            // Delete Table entity
            await _tableStorageService.DeleteProductAsync(partitionKey, rowKey);

            return RedirectToAction("Index");
        }


        public async Task<IActionResult> Index()
        {
            // Example of setting login status in ViewBag
            if (User.Identity.Name != null)
            {
                ViewBag.IsLoggedIn = true;
                var userName = User.Identity.Name;
                ViewBag.UserName = userName;

            }
            else
            {
                ViewBag.UserRole = "Guest";
                ViewBag.IsLoggedIn = false;
                return RedirectToAction("NeedToLogin", "Error");
            }


            // Get the current logged -in user's email or username from Identity
            var userEmail = User.Identity?.Name;


            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized();
            }

            // Fetch the user from Azure Table Storage based on their email (RowKey)
            var user = await _tableStorageService.GetUserByUsernameAsync(userEmail);

            if (user == null)
            {
                return Unauthorized("User not found in Azure Table Storage.");
            }

            // Check the role of the user
            if (user.Role == "Admin")
            {
                //Pass variable to view for display nar bar links
                ViewBag.UserRole = "Admin";

                var products = await _tableStorageService.GetAllProductsAsync();
                foreach (var product in products)
                {
                    Console.WriteLine($"Product: {product.Name}, Price: {product.Price}");
                }
                return View(products);
            }
            else if (user.Role == "Customer")
            {
                //Return customer narbar links
                ViewBag.UserRole = "Customer";
                return RedirectToAction("AccessDenied", "Error");
            }
            else
            {
                //return links for guests
                ViewBag.UserRole = "Guest";
                return RedirectToAction("NeedToLogin", "Error");
            }

            


        }

        [HttpGet]
        public async Task<IActionResult> Inventory()
        {
            if (User.Identity.Name != null)
            {
                ViewBag.IsLoggedIn = true;
                var userName = User.Identity.Name;
                ViewBag.UserName = userName;
            }
            else
            {
                ViewBag.UserRole = "Guest";
                ViewBag.IsLoggedIn = false;
                return RedirectToAction("NeedToLogin", "Error");
            }

            var userEmail = User.Identity?.Name;

            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized();
            }

            var user = await _tableStorageService.GetUserByUsernameAsync(userEmail);
            if (user == null)
            {
                return Unauthorized("User not found in Azure Table Storage.");
            }

            if (user.Role == "Admin")
            {
                //Pass variable to view for display nar bar links
                ViewBag.UserRole = "Admin";

                var products = await _tableStorageService.GetAllProductsAsync();
                return View(products);
            }
            else if (user.Role == "Customer")
            {
                //Return customer narbar links
                ViewBag.UserRole = "Customer";

                var products = await _tableStorageService.GetAllProductsAsync();
                return View(products);
            }
            else
            {
                //return links for guests
                ViewBag.UserRole = "Guest";
                return RedirectToAction("NeedToLogin", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Purchase(int ProductId, string CustomerEmail)
        {
            // Validate the user
            if (string.IsNullOrEmpty(CustomerEmail))
            {
                return Unauthorized("User not found.");
            }

            // Fetch the last order ID from the table storage
            int newOrderId = await GetNextOrderIdAsync();

            // Create a new order
            var newOrder = new Order
            {
                OrderId = newOrderId, // Set the new order ID
                PartitionKey = "OrdersPartition", // Define your partition key
                RowKey = Guid.NewGuid().ToString(), // Generate a unique row key
                CustomerEmail = CustomerEmail,
                ProductId = ProductId,
                OrderDate = DateTime.UtcNow,
                OrderStatus = "Pending"
            };

            // Save the order (implement this method in your TableStorageService)
            await _tableStorageService.AddOrderAsync(newOrder);
            // Set TempData to show confirmation modal
            TempData["OrderConfirmation"] = "Your order has been successfully placed.";

            // Serialize the order object into JSON format for further processing.
            var json = JsonConvert.SerializeObject
            (
                new Order // Create a new instance of the Order object for serialization.
                {
                    OrderId = newOrder.OrderId, // Assign the generated OrderId.
                    PartitionKey = newOrder.PartitionKey, // Include the PartitionKey.
                    RowKey = newOrder.RowKey, // Include the RowKey.
                    Timestamp = newOrder.Timestamp, // Include the timestamp of the order.
                    OrderDate = newOrder.OrderDate, // Include the order date.
                    CustomerEmail = newOrder.CustomerEmail, // Include the customer's email address.
                    ProductId = newOrder.ProductId, // Include the ID of the product being ordered.
                    OrderStatus = newOrder.OrderStatus // Include the Order Status
                }
            );

            // Prepare the JSON content for the HTTP POST request to the Azure function for order processing.
            var contentQueue = new StringContent(json, Encoding.UTF8, "application/json");
            // Send the serialized order to the specified Azure function via POST request.
            var response = await _httpClient.PostAsync("https://st10251759queuefunction.azurewebsites.net/api/ProcessOrders?code=smXxHVGJzs0pVflozwgbQYwJCnd-5p7gq6rvmisOo2DYAzFumTSTeg%3D%3D", contentQueue);
            // Ensure that the response indicates success. If it does not, an exception will be thrown.
            response.EnsureSuccessStatusCode();

           

            return RedirectToAction("Inventory","Products");
        }

        private async Task<int> GetNextOrderIdAsync()
        {
            var orders = await _tableStorageService.GetAllOrdersAsync();
            return orders.Any() ? orders.Max(o => o.OrderId) + 1 : 1;
        }

    }
}
