namespace ST10251759_CLDV6212_POE_Part_1.Models
{
    public class MyOrdersViewModel
    {
        public List<Order> Orders { get; set; }
        public Dictionary<int, string> ProductNames { get; set; }

        public MyOrdersViewModel()
        {
            Orders = new List<Order>();
            ProductNames = new Dictionary<int, string>();
        }
    }
}
