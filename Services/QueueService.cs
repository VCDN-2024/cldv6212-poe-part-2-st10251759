using Azure.Storage.Queues;

namespace ST10251759_CLDV6212_POE_Part_1.Services
{
    public class QueueService
    {
        private readonly string _connectionString;

        public QueueService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task SendMessageAsync(string queueName, string message)
        {
            // Create a QueueClient for the specified queue
            var queueClient = new QueueClient(_connectionString, queueName);

            // Ensure the queue exists
            await queueClient.CreateIfNotExistsAsync();

            // Send the message to the specified queue
            await queueClient.SendMessageAsync(message);
        }
    }
}
