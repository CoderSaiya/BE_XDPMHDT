﻿using FreelanceMarketplace.Data;
using FreelanceMarketplace.Models;
using FreelanceMarketplace.Models.DTOs.Req;
using FreelanceMarketplace.Services.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FreelanceMarketplace.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<Users> _passwordHasher;
        private readonly IEmailService _emailService;
        public UserService(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<Users>();
            _emailService = emailService;
        }

        public async Task<bool> RegisterUserAsync(RegisterReq registerReq)
        {
            var existingUser = await _context.Users.SingleOrDefaultAsync(u => u.Username == registerReq.Username);
            if (existingUser != null)
            {
                return false;
            }

            var user = new Users
            {
                Username = registerReq.Username,
                Email = registerReq.Email,
                Role = registerReq.Role,
                IsEmailConfirmed = false,
                EmailConfirmationToken = Guid.NewGuid().ToString(),
            };

            Console.WriteLine("User Email: " + user.Email);

            user.PasswordHash = _passwordHasher.HashPassword(user, registerReq.Password);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var confirmationLink = $"https://localhost:7115/api/Auth/confirm-email?userId={user.Id}&token={user.EmailConfirmationToken}";

            var emailBody = $@"
            <h2>Confirm your registration email</h2>
            <p>Hi, {user.Username}</p>
            <p>Please click the link below to confirm your account:</p>
            <a href='{confirmationLink}'>Confirm email</a>";

            await _emailService.SendEmailAsync(user.Email, "Xác nhận đăng ký tài khoản", emailBody);

            return true;
        }

        public async Task<bool> ConfirmEmailAsync(int userId, string token)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == userId && u.EmailConfirmationToken == token);
            if (user == null)
            {
                return false;
            }

            user.IsEmailConfirmed = true;
            user.EmailConfirmationToken = null;
            await _context.SaveChangesAsync();

            return true;
        }

        public Users Authenticate(string username, string password)
        {
            var user = _context.Users.SingleOrDefault(x => x.Username == username);
            if (user == null)
                return null;

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            if (result == PasswordVerificationResult.Success)
                return user;

            return null;
        }

        public void SaveRefreshToken(int userId, string refreshToken)
        {
            var token = new RefreshTokens
            {
                UserId = userId,
                Token = refreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsUsed = false,
                IsRevoked = false
            };

            _context.RefreshTokens.Add(token);
            _context.SaveChanges();
        }

        public RefreshTokens GetRefreshToken(int userId)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return _context.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked && !rt.IsUsed)
                .OrderByDescending(rt => rt.ExpiryDate)
                .FirstOrDefault();
#pragma warning restore CS8603 // Possible null reference return.
        }

        public RefreshTokens GetRefreshTokenByToken(string token)
        {
            return _context.RefreshTokens.SingleOrDefault(rt => rt.Token == token);
        }

        public void MarkRefreshTokenAsUsed(RefreshTokens refreshToken)
        {
            refreshToken.IsUsed = true;
            _context.RefreshTokens.Update(refreshToken);
            _context.SaveChanges();
        }

        public Users GetUserById(int userId)
        {
            return _context.Users.Find(userId);
        }

        public Users GetUserByUsername(string username)
        {
            return _context.Users.SingleOrDefault(u => u.Username == username);
        }

        public async Task<Users> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public List<Users> GetUsers()
        {
            return _context.Users.ToList();
        }

        public bool DeleteUserById(int userId)
        {
            var user = _context.Users.Find(userId);
            if (user == null) return false;

            _context.Users.Remove(user);
            _context.SaveChanges();
            return true;
        }
    }
}
