using ST10251759_CLDV6212_POE_Part_1.Models;

namespace ST10251759_CLDV6212_POE_Part_1.Services
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(string email, string password, string fullName);
        Task<User> LoginAsync(string email, string password);
    }
}
