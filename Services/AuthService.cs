using ST10251759_CLDV6212_POE_Part_1.Models;
using ST10251759_CLDV6212_POE_Part_1.Repositories;

namespace ST10251759_CLDV6212_POE_Part_1.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> RegisterAsync(string email, string password, string fullName)
        {
            //Check if user already exists
            var existingUser = await _userRepository.GetUserByEmailAsync(email);
            if (existingUser != null)
                return false;

            //Hash the prassword
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            //Create new user
            var user = new User
            {
                RowKey = email, //using email as Rowkey
                Email = email,
                FullName = fullName,
                PasswordHash = passwordHash,
                Role = "Customer"
            };

            return await _userRepository.CreateUserAsync(user);
        }

        public async Task<User> LoginAsync(string email, string password)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
                return null;

            bool isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            return isValid ? user : null;
        }
    }
}
