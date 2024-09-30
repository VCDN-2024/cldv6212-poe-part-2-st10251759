namespace ST10251759_CLDV6212_POE_Part_1.Models
{
    public class OrderViewModel
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public int OrderId { get; set; }
        public string CustomerEmail { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } // New property
        public DateTime? OrderDate { get; set; }
        public string OrderStatus { get; set; }
    }
}
