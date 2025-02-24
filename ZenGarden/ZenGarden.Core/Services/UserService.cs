using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Users>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task<Users?> GetUserByIdAsync(int userId)
        {
            return await _userRepository.GetByIdAsync(userId)
                   ?? throw new KeyNotFoundException($"User with ID {userId} not found.");
        }

        public async Task CreateUserAsync(Users user)
        {
            _ = user ?? throw new ArgumentNullException(nameof(user), "User cannot be null.");

            var existingUser = await _userRepository.GetByEmailAsync(user.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException($"User with email {user.Email} already exists.");
            }

            _userRepository.Create(user);
            if (await _unitOfWork.CommitAsync() == 0)
                throw new InvalidOperationException("Failed to create user.");
        }


        public async Task UpdateUserAsync(Users user)
        {
            _ = user ?? throw new ArgumentNullException(nameof(user), "User cannot be null.");
            
            _userRepository.Update(user);
            if (await _unitOfWork.CommitAsync() == 0)
                throw new InvalidOperationException("Failed to update user.");
        }

        public async Task DeleteUserAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId)
                       ?? throw new KeyNotFoundException($"User with ID {userId} not found.");

            _userRepository.Remove(user);
            if (await _unitOfWork.CommitAsync() == 0)
                throw new InvalidOperationException("Failed to delete user.");
        }

        public async Task<Users?> ValidateUserAsync(string? email, string? phone, string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty."); // Vẫn giữ check password

            return await _userRepository.ValidateUserAsync(email, phone, password);
        }
        
        public async Task<Users?> GetUserByRefreshTokenAsync(string refreshToken)
        {
            return await _userRepository.GetUserByRefreshTokenAsync(refreshToken);
        }
        
        public async Task UpdateUserRefreshTokenAsync(int userId, string refreshToken, DateTime expiryDate)
        {
            await _userRepository.UpdateUserRefreshTokenAsync(userId, refreshToken, expiryDate);
        }
    }
}