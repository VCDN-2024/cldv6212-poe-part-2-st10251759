using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;

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

namespace ST10251759_CLDV6212_POE_Part_1.Services
{
    public class BlobService
    {
       
        private readonly HttpClient _httpClient;


        public BlobService( HttpClient httpClient)
        {
            
            _httpClient = httpClient;

        }

        public async Task UploadAsync(Stream fileStream, string fileName)
        {
            var functionUrl = "https://blobfunction.azurewebsites.net/api/UploadToBlob?code=FPsB-nz91LH_QqXhH88WbqoBsZf66Dm6YCjViXrfK_DmAzFuaVAOEQ%3D%3D"; // Change to your function URL

            using var content = new StreamContent(fileStream);
            content.Headers.Add("file-name", fileName);  // Pass the file name in headers

            var response = await _httpClient.PostAsync(functionUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Failed to upload to Blob Storage");
            }
        }

        // Method to delete a blob by calling the Azure Function
        public async Task DeleteBlobAsync(string blobUri)
        {
            var functionUrl = $"https://blobfunction.azurewebsites.net/api/DeleteBlob?code=3ZJEpzgxvyVuuzDqR6v3vGTo06_pXtQEp5stgr2w2pD2AzFunljQOg%3D%3D&blobUri={Uri.EscapeDataString(blobUri)}";

            var response = await _httpClient.DeleteAsync(functionUrl);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Failed to delete the blob from Blob Storage");
            }
        }



    }
}
