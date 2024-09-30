using Azure.Storage.Files.Shares.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ST10251759_CLDV6212_POE_Part_1.Models;
using ST10251759_CLDV6212_POE_Part_1.Services;

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
    public class FilesController : Controller
    {
        private readonly AzureFileShareService _fileShareService;
        private readonly HttpClient _httpClient;
        private readonly TableStorageService _tableStorageService;

        public FilesController(AzureFileShareService fileShareService, HttpClient httpClient, TableStorageService tableStorageService)
        {
            _fileShareService = fileShareService;
            _httpClient = httpClient;
            _tableStorageService = tableStorageService;
        }

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
                ViewBag.UserRole = "Guest";
                ViewBag.IsLoggedIn = false;
                return View();
            }

            // Get the current logged -in user's email or username from Identity
            var userEmail = User.Identity?.Name;

            //check if user is logged in
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized();
            }

            // Fetch the user from Azure Table Storage based on their email (RowKey)
            var user = await _tableStorageService.GetUserByUsernameAsync(userEmail);

            if (user == null)
            {
                return View();
            }

            // Check the role of the user
            if (user.Role == "Admin")
            {
                //Pass variable to view for display nar bar links
                ViewBag.UserRole = "Admin";

                List<FileModel> files;
                try
                {
                    files = await _fileShareService.ListFilesAsync("uploads");
                }
                catch (Exception ex)
                {
                    ViewBag.Message = $"Failed to load files: {ex.Message}";
                    files = new List<FileModel>();
                }

                return View(files);

             
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

        //MVC Controller method to call the function thats uploads the file into Azure File Shares
        [HttpPost] // This attribute specifies that the method should respond to HTTP POST requests.
        public async Task<IActionResult> Upload(IFormFile file) // The method is asynchronous, returning a Task of type IActionResult. 
        {
            // Create a new instance of MultipartFormDataContent to hold the file content for the HTTP request.
            using (var fileContent = new MultipartFormDataContent())
            {
                // Open a read stream for the uploaded file. The IFormFile interface represents the file sent with the HTTP request.
                using (var stream = file.OpenReadStream())
                {
                    // Add the file stream to the MultipartFormDataContent. 
                    // The first parameter is the StreamContent that reads from the stream.
                    // The second parameter is the name of the form field that will be used on the server side ("file").
                    // The third parameter is the original file name provided by the user.
                    fileContent.Add(new StreamContent(stream), "file", file.FileName);

                    // Send an asynchronous POST request to the specified URL, 
                    // which is the Azure function endpoint that handles file uploads.
                    var fileResponse = await _httpClient.PostAsync(
                        "https://filesharefunction.azurewebsites.net/api/UploadFileToShare?code=MIHkePe-6pmyMKa6RhysnkTaFDFvb9ZWQt6lzaeveylAAzFuV8ERLw%3D%3D",
                        fileContent // The content of the request is the MultipartFormDataContent that contains the file.
                    );

                    // Check if the HTTP response status code indicates success (status code 2xx).
                    if (fileResponse.IsSuccessStatusCode)
                    {
                        // If the upload is successful, redirect the user to the "Index" action of the same controller.
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        // If the upload fails, return a BadRequest result with a message indicating failure.
                        return BadRequest("Failed upload File!");
                    }
                }
            }
        }


        // Handle file download
        [HttpGet]
        public async Task<IActionResult> DownloadFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("File name cannot be null or empty.");
            }

            try
            {
                var fileStream = await _fileShareService.DownloadFileAsync("uploads", fileName);

                if (fileStream == null)
                {
                    return NotFound($"File '{fileName}' not found.");
                }

                return File(fileStream, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error downloading file: {ex.Message}");
            }
        }
    }
}

