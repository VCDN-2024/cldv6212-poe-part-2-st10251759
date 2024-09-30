using Azure.Data.Tables;
using Azure;
using System.ComponentModel.DataAnnotations;

namespace ST10251759_CLDV6212_POE_Part_1.Models
{
    //CODE ATTRIBUTION
    //AUTHOR: Mick Gouweloos
    //SOURCE:https://github.com/mickymouse777/Cloud_Storage.git
    //URL:19 AUGUST 2024
    public class Order : ITableEntity
    {
        [Key]
        public int OrderId { get; set; }

        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        [Required(ErrorMessage = "Please select a customer.")]
        public string? CustomerEmail { get; set; } // FK to the Customer who made the order
        [Required(ErrorMessage = "Please select a product.")]
        public int ProductId { get; set; } // FK to the Product being ordered

        // [Required(ErrorMessage = "Please select the date.")]
        public DateTime? OrderDate { get; set; }


        public string? OrderStatus { get; set; } // FK to the Customer who made the order


    }
}
