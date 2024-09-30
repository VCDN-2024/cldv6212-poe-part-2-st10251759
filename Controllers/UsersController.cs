﻿using Microsoft.AspNetCore.Authorization;
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
    public class UsersController : Controller
    {
        private readonly TableStorageService _tableStorageService;

        public UsersController (TableStorageService tableStorageService)
        {
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

                //Return Users
                var users = await _tableStorageService.GetAllUsersAsync();
                return View(users);
            }
            else if (user.Role == "Customer")
            {
                //Return customer narbar links
                ViewBag.UserRole = "Customer";

                return View();
            }
            else
            {
                //return links for guests
                ViewBag.UserRole = "Guest";
                return View();
            }

            
        }

        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            // Delete customer from the table
            await _tableStorageService.DeleteUserAsync(partitionKey, rowKey);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Details(string partitionKey, string rowKey)
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

                // Fetch selected user for Admin
                var currentuser = await _tableStorageService.GetUserAsync(partitionKey, rowKey);
                if (currentuser == null)
                {
                    return NotFound();
                }
                return View(currentuser);
            }
            else if (user.Role == "Customer")
            {
                //Return customer narbar links
                ViewBag.UserRole = "Customer";

                return View();
            }
            else
            {
                //return links for guests
                ViewBag.UserRole = "Guest";
                return View();
            }

            
        }

        // GET: Users/Edit/{partitionKey}/{rowKey}
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

                var currentuser = await _tableStorageService.GetUserAsync(partitionKey, rowKey);
                if (currentuser == null)
                {
                    return NotFound();
                }
                return View(currentuser);
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

        // POST: Users/Edit/{partitionKey}/{rowKey}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string partitionKey, string rowKey, User user)
        {
            if (partitionKey != user.PartitionKey || rowKey != user.RowKey)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Use the new UpdateUserAsync method to update the customer in Azure Table Storage
                    await _tableStorageService.UpdateUserAsync(user);
                }
                catch
                {
                    // Handle errors if needed
                    return View(user);
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }
    }
}
