using ST10251759_CLDV6212_POE_Part_1.Models;

namespace ST10251759_CLDV6212_POE_Part_1.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetUserByEmailAsync(string email);
        Task<bool> CreateUserAsync(User user);
    }
}
