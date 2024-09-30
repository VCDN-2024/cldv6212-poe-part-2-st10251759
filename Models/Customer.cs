using Azure.Data.Tables;
using Azure;
using System.ComponentModel.DataAnnotations;

namespace ST10251759_CLDV6212_POE_Part_1.Models
{
    public class Customer : ITableEntity
    {
        [Key]
        public int CustomerId { get; set; }  // Ensure this property exists and is populated

        public string? CustomerName { get; set; }  // Ensure this property exists and is populated

        public string? Email { get; set; }

        public string? Password { get; set; }

        // ITableEntity implementation
        public string PartitionKey { get; set; }  // Typically used for logical partitioning, e.g., based on region or customer type
        public string RowKey { get; set; }  // Unique identifier, e.g., CustomerId.ToString()
        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }



    }
}
