using Azure.Data.Tables;
using Azure;
using System.ComponentModel.DataAnnotations;

namespace ST10251759_CLDV6212_POE_Part_1.Models
{
    public class User : ITableEntity
    {
        [Key]
        public string PartitionKey { get; set; } //Typically a fixed value, eg: "User"
        [Key]
        public string RowKey { get; set; } //Unique identifier, eg: username or email 

        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }

        public string Role { get; set; }

        //ITableEntity implementation
        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }

        public User()
        {

            PartitionKey = "User"; //Fixed partition Key
        }

    }
}
