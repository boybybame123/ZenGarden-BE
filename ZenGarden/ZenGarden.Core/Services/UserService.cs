﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;
using ZenGarden.Shared.Helpers;
using static System.Net.WebRequestMethods;

namespace ZenGarden.Core.Services;

public class UserService(
    IUserRepository userRepository,
    IBagRepository bagRepository,
    IWalletRepository walletRepository,
    IUserXpConfigRepository userXpConfigRepository,
    IUserExperienceRepository userExperienceRepository,
    IUserConfigRepository userConfigRepository,
    IUserTreeService userTreeService,
    IUserTreeRepository userTreeRepository,
    IBagItemRepository bagItemRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IUserService
{
    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var users = await userRepository.GetAllAsync();
        return mapper.Map<List<UserDto>>(users);
    }

    public async Task<Users?> GetUserByIdAsync(int userId)
    {
        return await userRepository.GetByIdAsync(userId)
               ?? throw new KeyNotFoundException($"User with ID {userId} not found.");
    }


    public async Task<Users?> GetUserByEmailAsync(string email)
    {
        return await userRepository.GetByEmailAsync(email);
    }


    public async Task UpdateUserAsync(UpdateUserDTO user)
    {
        var userUpdate = await GetUserByIdAsync(user.UserId);
        if (userUpdate == null)
            throw new KeyNotFoundException($"User with ID {user.UserId} not found.");

        if (!string.IsNullOrEmpty(user.Password))
            userUpdate.Password = PasswordHasher.HashPassword(user.Password);
        if (!string.IsNullOrEmpty(user.Email))
            userUpdate.Email = user.Email;
        if (!string.IsNullOrEmpty(user.Phone))
            userUpdate.Phone = user.Phone;
        if (!string.IsNullOrEmpty(user.UserName))
            userUpdate.UserName = user.UserName;
        if (user.RoleId != null && user.RoleId != 0)
            userUpdate.RoleId = user.RoleId;

        if (user.Status != userUpdate.Status)
            userUpdate.Status = user.Status;


        userRepository.Update(userUpdate);
        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to update user.");
    }


    public async Task DeleteUserAsync(int userId)
    {
        var user = await userRepository.GetByIdAsync(userId)
                   ?? throw new KeyNotFoundException($"User with ID {userId} not found.");

        await userRepository.RemoveAsync(user);
        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to delete user.");
    }

    public async Task<Users?> ValidateUserAsync(string? email, string? phone, string password)
    {
        var user = !string.IsNullOrEmpty(email)
            ? await userRepository.GetByEmailAsync(email)
            : await userRepository.GetByPhoneAsync(phone);

        if (user == null || string.IsNullOrEmpty(user.Password)) return null;

        return PasswordHasher.VerifyPassword(password, user.Password) ? user : null;
    }

    public async Task<Users?> GetUserByRefreshTokenAsync(string refreshToken)
    {
        return await userRepository.GetUserByRefreshTokenAsync(refreshToken);
    }

    public async Task UpdateUserRefreshTokenAsync(int userId, string refreshToken, DateTime expiryDate)
    {
        await userRepository.UpdateUserRefreshTokenAsync(userId, refreshToken, expiryDate);
    }

    public async Task RemoveRefreshTokenAsync(int userId)
    {
        var user = await userRepository.GetByIdAsync(userId)
                   ?? throw new KeyNotFoundException("User not found.");

        user.RefreshTokenHash = null;
        user.RefreshTokenExpiry = DateTime.UtcNow;

        await unitOfWork.CommitAsync();
    }

    public async Task<Users?> RegisterUserAsync(RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Password))
            throw new ArgumentException("Password cannot be empty.");

        var existingUser = await userRepository.GetByEmailAsync(dto.Email);
        if (existingUser != null) throw new InvalidOperationException("Email is already in use.");

        var role = await userRepository.GetRoleByIdAsync(dto.RoleId ?? 2)
                   ?? throw new InvalidOperationException("Invalid RoleId.");

        var newUser = mapper.Map<Users>(dto);

        newUser.UserName = string.IsNullOrWhiteSpace(dto.UserName) ? GenerateRandomUsername() : dto.UserName;

        newUser.Password = PasswordHasher.HashPassword(dto.Password);
        newUser.RoleId = role.RoleId;
        newUser.Status = UserStatus.Active;


        await unitOfWork.BeginTransactionAsync();
        try
        {
            await userRepository.CreateAsync(newUser);
            await unitOfWork.CommitAsync();

            var wallet = new Wallet { UserId = newUser.UserId, Balance = 0 };
            var bag = new Bag { UserId = newUser.UserId };

            var levelOneConfig = await userXpConfigRepository.GetByIdAsync(1)
                                 ?? throw new Exception("UserLevelConfig is missing Level 1!");

            var userExperience = new UserExperience
            {
                UserId = newUser.UserId,
                TotalXp = 0,
                XpToNextLevel = levelOneConfig.XpThreshold,
                LevelId = 1,
                IsMaxLevel = false,
                StreakDays = 0,
                CreatedAt = DateTime.UtcNow
            };
            var userConfig = new UserConfig
            {
                UserId = newUser.UserId,
                BackgroundConfig = "https://hcm.ss.bfcplatform.vn/zengarden/OIP.jpg?AWSAccessKeyId=8QEUOTPT6CM3J3X9CD9T&Expires=1743441883&Signature=LNL9h7zMH2%2BeZpQdBdDhnoCVi1k%3D",
                SoundConfig = "https://hcm.ss.bfcplatform.vn/zengarden/sound-design-elements-sfx-ps-022-302865.mp3?AWSAccessKeyId=8QEUOTPT6CM3J3X9CD9T&Expires=1743441777&Signature=mMR0vRZRqJQvQmeJ4qYTh6xMkp0%3D",
                ImageUrl = "https://hcm.ss.bfcplatform.vn/zengarden/male.png?AWSAccessKeyId=8QEUOTPT6CM3J3X9CD9T&Expires=1743441822&Signature=DJNiWS8ebIWoyCqDEqfccJocY5I%3D",
                CreatedAt = DateTime.UtcNow
            };
            
            await walletRepository.CreateAsync(wallet);
            await bagRepository.CreateAsync(bag);
            await userExperienceRepository.CreateAsync(userExperience);
            await userConfigRepository.CreateAsync(userConfig);
            await unitOfWork.CommitAsync();
            await unitOfWork.CommitTransactionAsync();
            return newUser;
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task MakeItemdefault(int userid)
    {
        if (userid == 0)
            throw new ArgumentException("User Id cannot be empty.");

        var bag = await bagRepository.GetByUserIdAsync(userid)
                  ?? throw new KeyNotFoundException($"Bag for user with ID {userid} not found.");

        var itembag = new BagItem
        {
            BagId = bag.BagId,
            ItemId = 1,
            Quantity = 1,
            CreatedAt = DateTime.UtcNow
        };

        await bagItemRepository.CreateAsync(itembag);
        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to create item in bag.");
    }






    public async Task<string> GenerateAndSaveOtpAsync(string email)
    {
        var user = await userRepository.GetByEmailAsync(email)
                   ?? throw new KeyNotFoundException("User not found.");

        var otp = new Random().Next(100000, 999999).ToString();

        user.OtpCodeHash = PasswordHasher.HashPassword(otp);
        user.OtpExpiry = DateTime.UtcNow.AddMinutes(5);

        await unitOfWork.CommitAsync();
        return otp;
    }

    public async Task<bool> ResetPasswordAsync(string email, string otp, string newPassword)
    {
        var user = await userRepository.GetByEmailAsync(email);
        if (user?.OtpCodeHash == null || user.OtpExpiry < DateTime.UtcNow) return false;

        var isValid = PasswordHasher.VerifyPassword(otp, user.OtpCodeHash);
        if (!isValid) return false;

        user.Password = PasswordHasher.HashPassword(newPassword);
        user.OtpCodeHash = null;
        user.OtpExpiry = null;

        await unitOfWork.CommitAsync();
        return true;
    }

    public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        if (!PasswordHasher.VerifyPassword(oldPassword, user.Password)) return false;

        user.Password = PasswordHasher.HashPassword(newPassword);
        await unitOfWork.CommitAsync();
        return true;
    }

    public async Task OnUserLoginAsync(int userId)
    {
        var userTrees = await userTreeRepository.GetUserTreeByUserIdAsync(userId);

        foreach (var tree in userTrees) await userTreeService.UpdateSpecificTreeHealthAsync(tree.UserTreeId);
    }

    public async Task<List<Users>> GetAllUserFilterAsync(UserFilterDto filter)
    {
        var userFilterResult = await userRepository.GetAllAsync(filter);
        if (userFilterResult.Data == null)
            throw new KeyNotFoundException("User not found.");
        return userFilterResult.Data;
    }

    public async Task CreateUserAsync(Users user)
    {
        var existingUser = await userRepository.GetByEmailAsync(user.Email);
        if (existingUser != null)
            throw new InvalidOperationException($"User with email {user.Email} already exists.");

        await userRepository.CreateAsync(user);
        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to create user.");
    }

    private static string GenerateRandomUsername()
    {
        return $"User_{Guid.NewGuid().ToString("N")[..8]}";
    }
}