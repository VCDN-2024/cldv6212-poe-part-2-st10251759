using Azure.Data.Tables;
using Azure;
using ST10251759_CLDV6212_POE_Part_1.Models;

namespace ST10251759_CLDV6212_POE_Part_1.Services
{
    public class TableStorageService
    {
        private readonly TableClient _productTableClient;
        private readonly TableClient _customerTableClient;
        private readonly TableClient _orderTableClient;
        private readonly TableClient _usersTableClient;

        public TableStorageService(string connectionString)
        {
            _productTableClient = new TableClient(connectionString, "Products");
            _customerTableClient = new TableClient(connectionString, "Customers");
            _orderTableClient = new TableClient(connectionString, "Orders");
            _usersTableClient = new TableClient(connectionString, "Users");
        }

        // Product Operations
        public async Task<List<Product>> GetAllProductsAsync()
        {
            var products = new List<Product>();
            await foreach (var product in _productTableClient.QueryAsync<Product>())
            {
                products.Add(product);
            }
            return products;
        }

        public async Task<Product> GetProductByIdAsync(int productId)
        {
            // Define the filter expression to query the ProductId (RowKey)
            string filter = TableClient.CreateQueryFilter<Product>(p => p.RowKey == productId.ToString());

            // Query across all partition keys
            var products = _productTableClient.QueryAsync<Product>(filter);

            // Find the first matching product
            await foreach (var product in products)
            {
                return product; // Return the first match
            }

            // Return null if no product is found
            return null;
        }


        public async Task AddProductAsync(Product product)
        {
            if (string.IsNullOrEmpty(product.PartitionKey) || string.IsNullOrEmpty(product.RowKey))
            {
                throw new ArgumentException("PartitionKey and RowKey must be set.");
            }

            try
            {
                await _productTableClient.AddEntityAsync(product);
            }
            catch (RequestFailedException ex)
            {
                throw new InvalidOperationException("Error adding product to Table Storage", ex);
            }
        }

        public async Task DeleteProductAsync(string partitionKey, string rowKey)
        {
            await _productTableClient.DeleteEntityAsync(partitionKey, rowKey);
        }

        public async Task<Product?> GetProductAsync(string partitionKey, string rowKey)
        {
            try
            {
                var response = await _productTableClient.GetEntityAsync<Product>(partitionKey, rowKey);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        public async Task<IEnumerable<Product>> GetProductsBySearchTermAsync(string searchTerm)
        {
            // Fetch all products and filter them based on the search term
            var allProducts = await GetAllProductsAsync();
            return allProducts.Where(p => p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<Dictionary<int, string>> GetProductNamesAsync()
        {
            var productNames = new Dictionary<int, string>();
            await foreach (var product in _productTableClient.QueryAsync<Product>()) // Assuming you have a Product model
            {
                productNames[product.ProductId] = product.Name; // Use Product_Id as the key
            }
            return productNames;
        }


        // Add this new method
        public async Task<Product?> GetProductByIdAsync(string productId)
        {
            // Assuming ProductId is the RowKey and the partition key is fixed
            try
            {
                // Replace "ProductsPartition" with your actual partition key if it’s different
                var response = await _productTableClient.GetEntityAsync<Product>("ProductsPartition", productId);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        // Customer Operations
        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            var customers = new List<Customer>();
            await foreach (var customer in _customerTableClient.QueryAsync<Customer>())
            {
                customers.Add(customer);
            }
            return customers;
        }

        public async Task AddCustomerAsync(Customer customer)
        {
            if (string.IsNullOrEmpty(customer.PartitionKey) || string.IsNullOrEmpty(customer.RowKey))
            {
                throw new ArgumentException("PartitionKey and RowKey must be set.");
            }

            try
            {
                await _customerTableClient.AddEntityAsync(customer);
            }
            catch (RequestFailedException ex)
            {
                throw new InvalidOperationException("Error adding customer to Table Storage", ex);
            }
        }

        public async Task DeleteCustomerAsync(string partitionKey, string rowKey)
        {
            await _customerTableClient.DeleteEntityAsync(partitionKey, rowKey);
        }

        public async Task<Customer?> GetCustomerAsync(string partitionKey, string rowKey)
        {
            try
            {
                var response = await _customerTableClient.GetEntityAsync<Customer>(partitionKey, rowKey);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        // Order Operations
        public async Task<List<Order>> GetAllOrdersAsync()
        {
            var orders = new List<Order>();
            await foreach (var order in _orderTableClient.QueryAsync<Order>())
            {
                orders.Add(order);
            }
            return orders;
        }

        public async Task<Order> GetOrdersAsync(string partitionKey, string rowKey)
        {

            var order = await _orderTableClient.GetEntityAsync<Order>(partitionKey, rowKey);

            // Return the entity if it exists
            return order.Value;
        }


        public async Task AddOrderAsync(Order order)
        {
            if (string.IsNullOrEmpty(order.PartitionKey) || string.IsNullOrEmpty(order.RowKey))
            {
                throw new ArgumentException("PartitionKey and RowKey must be set.");
            }

            try
            {
                await _orderTableClient.AddEntityAsync(order);
            }
            catch (RequestFailedException ex)
            {
                throw new InvalidOperationException("Error adding order to Table Storage", ex);
            }
        }

        public async Task<List<Order>> GetOrdersByUserEmailAsync(string email)
        {
            var orders = new List<Order>();
            await foreach (var order in _orderTableClient.QueryAsync<Order>(o => o.CustomerEmail == email))
            {
                orders.Add(order);
            }
            return orders;
        }

        public async Task UpdateOrderAsync(Order order)
        {
            if (string.IsNullOrEmpty(order.PartitionKey) || string.IsNullOrEmpty(order.RowKey))
            {
                throw new ArgumentException("PartitionKey and RowKey must be set.");
            }

            try
            {
                // Update the order in the Azure Table Storage
                await _orderTableClient.UpsertEntityAsync(order, TableUpdateMode.Replace);
            }
            catch (RequestFailedException ex)
            {
                throw new InvalidOperationException("Error updating order in Table Storage", ex);
            }
        }

        public async Task DeleteOrderAsync(string partitionKey, string rowKey)
        {
            await _orderTableClient.DeleteEntityAsync(partitionKey, rowKey);
        }

        public async Task<Order?> GetOrderAsync(string partitionKey, string rowKey)
        {
            try
            {
                // Fetch the order entity using partitionKey and rowKey
                var response = await _orderTableClient.GetEntityAsync<Order>(partitionKey, rowKey);

                // Return the order entity itself
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                // Return null if the entity is not found (404 error)
                return null;
            }
        }

        public async Task UpdateCustomerAsync(Customer customer)
        {
            if (string.IsNullOrEmpty(customer.PartitionKey) || string.IsNullOrEmpty(customer.RowKey))
            {
                throw new ArgumentException("PartitionKey and RowKey must be set.");
            }

            try
            {
                // Update the customer in the Azure Table Storage
                await _customerTableClient.UpsertEntityAsync(customer, TableUpdateMode.Replace);
            }
            catch (RequestFailedException ex)
            {
                throw new InvalidOperationException("Error updating customer in Table Storage", ex);
            }
        }

        // User Operations
        public async Task<List<User>> GetAllUsersAsync()
        {
            var users = new List<User>();
            await foreach (var user in _usersTableClient.QueryAsync<User>())
            {
                users.Add(user);
            }
            return users;
        }

        public async Task AddUserAsync(User user)
        {
            if (string.IsNullOrEmpty(user.PartitionKey) || string.IsNullOrEmpty(user.RowKey))
            {
                throw new ArgumentException("PartitionKey and RowKey must be set.");
            }

            try
            {
                await _usersTableClient.AddEntityAsync(user);
            }
            catch (RequestFailedException ex)
            {
                throw new InvalidOperationException("Error adding user to Table Storage", ex);
            }
        }

        public async Task DeleteUserAsync(string partitionKey, string rowKey)
        {
            await _usersTableClient.DeleteEntityAsync(partitionKey, rowKey);
        }

        public async Task<User?> GetUserAsync(string partitionKey, string rowKey)
        {
            try
            {
                var response = await _usersTableClient.GetEntityAsync<User>(partitionKey, rowKey);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            try
            {
                // Querying the Users table to find the user by their FullName (username)
                var queryResults = _usersTableClient.QueryAsync<User>(u => u.FullName == username);

                // Retrieve the first user found that matches the username
                await foreach (var user in queryResults)
                {
                    return user; // Return the user if found
                }

                // If no user is found, return null
                return null;
            }
            catch (RequestFailedException ex)
            {
                throw new InvalidOperationException("Error retrieving user from Table Storage", ex);
            }
        }

        public async Task UpdateUserAsync(User user)
        {
            if (string.IsNullOrEmpty(user.PartitionKey) || string.IsNullOrEmpty(user.RowKey))
            {
                throw new ArgumentException("PartitionKey and RowKey must be set.");
            }

            try
            {
                // Update the customer in the Azure Table Storage
                await _usersTableClient.UpsertEntityAsync(user, TableUpdateMode.Replace);
            }
            catch (RequestFailedException ex)
            {
                throw new InvalidOperationException("Error updating user in Table Storage", ex);
            }
        }
    }
}
