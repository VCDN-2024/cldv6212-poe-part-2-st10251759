using Azure.Data.Tables;
using Azure;
using ST10251759_CLDV6212_POE_Part_1.Models;

namespace ST10251759_CLDV6212_POE_Part_1.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly TableClient _tableClient;

        public UserRepository(IConfiguration configuration)
        {
            var connectionString = configuration.GetSection("AzureTableStorage")["ConnectionString"];
            var tableName = configuration.GetSection("AzureTableStorage")["TableName"];
            _tableClient = new TableClient(connectionString, tableName);
            _tableClient.CreateIfNotExists();
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            try
            {
                //Assuming Rowkey is email
                var response = await _tableClient.GetEntityAsync<User>("User", email);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        public async Task<bool> CreateUserAsync(User user)
        {
            try
            {
                await _tableClient.AddEntityAsync(user);
                return true;
            }
            catch (RequestFailedException)
            {
                //handle exceptions (eg: duplicate key)
                return false;
            }
        }
    }
}
