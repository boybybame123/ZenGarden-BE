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
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found."); // ✅ Middleware sẽ bắt lỗi 404

            return user;
        }

        public async Task CreateUserAsync(Users user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user), "User cannot be null.");

            _userRepository.Create(user);
            var result = await _unitOfWork.CommitAsync();

            if (result == 0)
                throw new InvalidOperationException("Failed to create user.");
        }

        public async Task UpdateUserAsync(Users user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user), "User cannot be null.");

            _userRepository.Update(user);
            var result = await _unitOfWork.CommitAsync();

            if (result == 0)
                throw new InvalidOperationException("Failed to update user.");
        }

        public async Task DeleteUserAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found.");

            _userRepository.Remove(user);
            var result = await _unitOfWork.CommitAsync();

            if (result == 0)
                throw new InvalidOperationException("Failed to delete user.");
        }


        public async Task<Users?> ValidateUserAsync(string? email, string? phone, string password)
        {
            return await _userRepository.ValidateUserAsync(email, phone, password);
        }
    }
}
