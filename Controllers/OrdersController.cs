using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

 Author: Microsoft
 Link: https://learn.microsoft.com/en-us/azure/azure-functions/functions-overview?pivots=programming-language-csharp
 Date Accessed: 25 September 2024

 Author: Microsoft
 Link: https://learn.microsoft.com/en-us/azure/service-connector/quickstart-portal-functions-connection?tabs=SMI
 Date Accessed: 25 September 2024

 Author: Microsoft
 Link: https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-blob?tabs=isolated-process%2Cextensionv5%2Cextensionv3&pivots=programming-language-csharp
 Date Accessed: 25 September 2024

 Author: Microsoft
 Link: https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-queue-trigger?tabs=python-v2%2Cisolated-process%2Cnodejs-v4%2Cextensionv5&pivots=programming-language-csharp 
 Date Accessed: 25 September 2024

 Author: Microsoft
 Link: https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-table?tabs=isolated-process%2Ctable-api%2Cextensionv3&pivots=programming-language-csharp
 Date Accessed: 25 September 2024


 *********All Images used throughout project are adapted from Pinterest (https://www.furiousmotorsport.com/)*************

 ==============================Code Attribution==================================
     
 */

namespace ST10251759_CLDV6212_POE_Part_1.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly TableStorageService _tableStorageService;
        private readonly QueueService _queueService;
        private readonly HttpClient _httpClient;

        public OrdersController(TableStorageService tableStorageService, QueueService queueService, HttpClient httpClient)
        {
            _tableStorageService = tableStorageService;
            _queueService = queueService;
            _httpClient = httpClient;
        }

        // Action to display all orders
        public async Task<IActionResult> Index()
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
                ViewBag.IsLoggedIn = false;
                return RedirectToAction("NeedToLogin", "Error");
            }

            // Get the current logged -in user's email or username from Identity
            var userEmail = User.Identity?.Name;


            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("NeedToLogin", "Error");

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

                // Fetch all orders for Admin
                var orders = await _tableStorageService.GetAllOrdersAsync();
                return View("Index", orders); // Return a view for Admins
            }
            else if (user.Role == "Customer")
            {
                ViewBag.UserRole = "Customer";

                // Access Denied
                return RedirectToAction("AccessDenied", "Error");
            }
            else
            {
                ViewBag.UserRole = "Guest";
                return RedirectToAction("NeedToLogin", "Error");
            }
        }

        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            // Delete Table entity
            await _tableStorageService.DeleteOrderAsync(partitionKey, rowKey);

            return RedirectToAction("Index");
        }




        // Action to display the form for creating a new order
        public async Task<IActionResult> Create()
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
                ViewBag.IsLoggedIn = false;
            }



            // Get the current logged -in user's email or username from Identity
            var userEmail = User.Identity?.Name;


            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("NeedToLogin", "Error");

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

                try
                {
                    var users = await _tableStorageService.GetAllUsersAsync();
                    var products = await _tableStorageService.GetAllProductsAsync();

                    ViewData["Users"] = new SelectList(users, "Email", "FullName");
                    ViewData["Products"] = new SelectList(products, "ProductId", "Name");
                    ViewBag.OrderStatusList = new SelectList(new List<string> { "Pending", "Processed", "Delivered" });

                    return View();
                }
                catch (Exception ex)
                {
                    // Log the exception
                    Console.WriteLine($"An error occurred while loading the create page: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while loading the create page.");
                    return View();
                }
            }
            else if (user.Role == "Customer")
            {
                ViewBag.UserRole = "Customer";

                // Access Denied
                return RedirectToAction("AccessDenied", "Error");
            }
            else
            {
                ViewBag.UserRole = "Guest";
                return RedirectToAction("NeedToLogin", "Error");
            }

            
        }

        // Action to handle the form submission and create a new order - Call Queue Function to add to queue
        [HttpPost] // This attribute specifies that the method responds to HTTP POST requests, typically used for form submissions.
        public async Task<IActionResult> Create(Order order) // The method is asynchronous and returns a Task of type IActionResult.
        {
            // Retrieve the product details from the table storage using the provided ProductId from the order.
            var product = await _tableStorageService.GetProductAsync("ProductsPartition", order.ProductId.ToString());

            // Check if the model state is valid, which means that all validation attributes on the Order model are satisfied.
            if (ModelState.IsValid)
            {
                try
                {
                    // Ensure the Order_Date is not null. This is a crucial validation check before processing the order.
                    if (!order.OrderDate.HasValue)
                    {
                        // If the Order_Date is null, add a model error indicating that the Order Date is required.
                        ModelState.AddModelError("OrderDate", "Order Date is required.");
                        // Return the view with the current order data to inform the user of the error.
                        return View(order);
                    }

                    // Specify that the OrderDate is in UTC format, ensuring consistency across time zones.
                    order.OrderDate = DateTime.SpecifyKind(order.OrderDate.Value, DateTimeKind.Utc);

                    // Generate a new unique Order_Id. Retrieve all existing orders to determine the next available OrderId.
                    var allOrders = await _tableStorageService.GetAllOrdersAsync();
                    order.OrderId = (allOrders.Count > 0) ? allOrders.Max(o => o.OrderId) + 1 : 1; // Assign a new OrderId based on the highest existing OrderId or start at 1.

                    // Set the PartitionKey and RowKey for the order entry in table storage.
                    order.PartitionKey = "OrdersPartition"; // Use a specific partition for order storage.
                    order.RowKey = Guid.NewGuid().ToString(); // Generate a new unique RowKey using GUID.

                    // Add the new order to the table storage.
                    await _tableStorageService.AddOrderAsync(order);

                    // Serialize the order object into JSON format for further processing.
                    var json = JsonConvert.SerializeObject
                    (
                        new Order // Create a new instance of the Order object for serialization.
                        {
                            OrderId = order.OrderId, // Assign the generated OrderId.
                            PartitionKey = order.PartitionKey, // Include the PartitionKey.
                            RowKey = order.RowKey, // Include the RowKey.
                            Timestamp = order.Timestamp, // Include the timestamp of the order.
                            OrderDate = order.OrderDate, // Include the order date.
                            CustomerEmail = order.CustomerEmail, // Include the customer's email address.
                            ProductId = order.ProductId, // Include the ID of the product being ordered.
                            OrderStatus = order.OrderStatus // Include the Order Status
                        }
                    );

                    // Prepare the JSON content for the HTTP POST request to the Azure function for order processing.
                    var contentQueue = new StringContent(json, Encoding.UTF8, "application/json");
                    // Send the serialized order to the specified Azure function via POST request.
                    var response = await _httpClient.PostAsync("https://st10251759queuefunction.azurewebsites.net/api/ProcessOrders?code=smXxHVGJzs0pVflozwgbQYwJCnd-5p7gq6rvmisOo2DYAzFumTSTeg%3D%3D", contentQueue);
                    // Ensure that the response indicates success. If it does not, an exception will be thrown.
                    response.EnsureSuccessStatusCode();

                    // Redirect the user to the Index action upon successful order creation.
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Catch any exceptions that occur during the order processing.
                    // Log the exception message to the console for debugging purposes.
                    Console.WriteLine($"An error occurred while processing the order: {ex.Message}");
                    // Add a generic error message to the model state to inform the user of the failure.
                    ModelState.AddModelError("", "An error occurred while processing your order.");
                }
            }

            // Reload the data in case of error to ensure the user has access to the latest data.
            try
            {
                // Retrieve all users from table storage to populate the dropdown list in the view.
                var users = await _tableStorageService.GetAllUsersAsync();
                // Retrieve all products to populate the product dropdown in the view.
                var products = await _tableStorageService.GetAllProductsAsync();

                // Use ViewData to pass the users and products lists to the view for rendering.
                ViewData["Users"] = new SelectList(users, "Email", "FullName"); // Populate users for selection based on Email and FullName.
                ViewData["Products"] = new SelectList(products, "Product_Id", "Name", order.ProductId); // Populate products for selection.
            }
            catch (Exception ex)
            {
                // Catch any exceptions that occur while reloading data.
                // Log the exception message to the console for debugging purposes.
                Console.WriteLine($"An error occurred while reloading data: {ex.Message}");
                // Ensure the Products ViewData is set to an empty list in case of an error.
                ViewData["Products"] = new SelectList(new List<Product>(), "Product_Id", "Name");
            }

            // Return the view with the current order object, including any model state errors.
            return View(order);
        }

        private async Task<IActionResult> ReloadCreateView(Order order)
        {
            try
            {
                var users = await _tableStorageService.GetAllUsersAsync();
                var products = await _tableStorageService.GetAllProductsAsync();

                ViewData["Users"] = new SelectList(users, "Email", "FullName", order.CustomerEmail);
                ViewData["Products"] = new SelectList(products, "ProductId", "Name", order.ProductId);
                ViewBag.OrderStatusList = new SelectList(new List<string> { "Pending", "Processed", "Delivered" }, order.OrderStatus);

                return View(order);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reloading create view: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while reloading the create view.");
                return View(order);
            }
        }

 

        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
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
                ViewBag.IsLoggedIn = false;
                return RedirectToAction("NeedToLogin", "Error");
            }

            // Get the current logged -in user's email or username from Identity
            var userEmail = User.Identity?.Name;


            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("NeedToLogin", "Error");

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

                
                var order = _tableStorageService.GetOrderAsync(partitionKey, rowKey); // Fetch the order by PartitionKey and RowKey

                // Fetch the product name based on ProductId
                var product = _tableStorageService.GetProductByIdAsync(order.Result.ProductId);
                var productName = product?.Result.Name ?? "Unknown Product";

                // Create the ViewModel
                var viewModel = new OrderViewModel
                {
                    PartitionKey = order.Result.PartitionKey,
                    RowKey = order.Result.RowKey,
                    OrderId = order.Result.OrderId,
                    CustomerEmail = order.Result.CustomerEmail,
                    ProductId = order.Result.ProductId,
                    ProductName = productName, // Pass the product name to the ViewModel
                    OrderDate = order.Result.OrderDate,
                    OrderStatus = order.Result.OrderStatus
                };

                ViewBag.OrderStatusList = new SelectList(new List<string> { "Pending", "Processed", "Delivered" }, viewModel.OrderStatus);

                return View(viewModel);
            }
            else if (user.Role == "Customer")
            {
                ViewBag.UserRole = "Customer";

                // Access Denied
                return RedirectToAction("AccessDenied", "Error");
            }
            else
            {
                ViewBag.UserRole = "Guest";
                return RedirectToAction("NeedToLogin", "Error");
            }


        }

        // Action to handle the form submission for updating the order
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string partitionKey, string rowKey, Order updatedOrder)
        {
            if (!ModelState.IsValid)
            {
                // If the model is not valid, reload the form with the same data
                ViewBag.OrderStatusList = new SelectList(new List<string> { "Pending", "Processed", "Delivered" }, updatedOrder.OrderStatus);
                return View(updatedOrder);
            }

            // Fetch the existing order from Table Storage to update only the OrderStatus
            var order = await _tableStorageService.GetOrderAsync(partitionKey, rowKey);

            if (order == null)
            {
                return NotFound();
            }

            // Update only the OrderStatus
            order.OrderStatus = updatedOrder.OrderStatus;

            // Save the updated order back to Table Storage
            await _tableStorageService.UpdateOrderAsync(order);

            // Redirect to the order index after successful update
            return RedirectToAction(nameof(Index));
        }



        // Helper function to fetch customer-specific orders
        private async Task<List<Order>> GetOrdersForCustomer(string customerEmail)
        {
            var allOrders = await _tableStorageService.GetAllOrdersAsync();
            // Assuming orders contain a customer email, filter orders for the logged-in customer
            return allOrders.Where(o => o.CustomerEmail == customerEmail).ToList();
        }



        public async Task<IActionResult> MyOrders()
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
                ViewBag.IsLoggedIn = false;
                return RedirectToAction("NeedToLogin", "Error");
            }

            // Get the current logged -in user's email or username from Identity
            var userEmail = User.Identity?.Name;


            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("NeedToLogin", "Error");

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

                if (string.IsNullOrEmpty(userEmail))
                {
                    return Unauthorized();
                }

                // Retrieve all orders placed by the logged-in user
                var orders = await _tableStorageService.GetOrdersByUserEmailAsync(userEmail);

                // Retrieve all product names in a dictionary
                var productNames = await _tableStorageService.GetProductNamesAsync();

                // Create the view model
                var viewModel = new MyOrdersViewModel
                {
                    Orders = orders.ToList(),
                    ProductNames = productNames
                };

                return View(viewModel); // Pass the view model to the view
            }
            else if (user.Role == "Customer")
            {
                ViewBag.UserRole = "Customer";
                if (string.IsNullOrEmpty(userEmail))
                {
                    return Unauthorized();
                }

                // Retrieve all orders placed by the logged-in user
                var orders = await _tableStorageService.GetOrdersByUserEmailAsync(userEmail);

                // Retrieve all product names in a dictionary
                var productNames = await _tableStorageService.GetProductNamesAsync();

                // Create the view model
                var viewModel = new MyOrdersViewModel
                {
                    Orders = orders.ToList(),
                    ProductNames = productNames
                };

                return View(viewModel); // Pass the view model to the view
            }
            else
            {
                ViewBag.UserRole = "Guest";
                return RedirectToAction("NeedToLogin", "Error");
            }

            
        }

    }

}
