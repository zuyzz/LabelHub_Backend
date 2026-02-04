using DataLabel_Project_BE.Data;
using DataLabel_Project_BE.DTOs.Auth;
using DataLabel_Project_BE.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DataLabel_Project_BE.Services
{
    /// <summary>
    /// Authentication service for handling user login and management
    /// 
    /// DATABASE IMPLEMENTATION (PostgreSQL - Supabase):
    /// - Uses EF Core with Npgsql
    /// - Role table is pre-seeded (4 fixed roles)
    /// - Default password for new users: configured in appsettings.json
    /// 
    /// Implements password hashing using SHA256 and JWT token generation
    /// </summary>
    public class AuthService
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public AuthService(IConfiguration configuration, AppDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        /// <summary>
        /// Hash password using SHA256
        /// </summary>
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        /// <summary>
        /// Verify password against stored hash
        /// </summary>
        private bool VerifyPassword(string password, string passwordHash)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == passwordHash;
        }

        /// <summary>
        /// Login method - validates user credentials and generates JWT token
        /// Validates username/email, password, and active status
        /// Returns specific error messages for different failure scenarios
        /// </summary>
        public async Task<(LoginResponse? Response, string? ErrorMessage)> Login(LoginRequest request)
        {
            // Find user by username or email (case-insensitive)
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u =>
                    u.Username.ToLower() == request.UsernameOrEmail.ToLower() ||
                    (u.Email != null && u.Email.ToLower() == request.UsernameOrEmail.ToLower()));

            // Check if user exists
            if (user == null)
            {
                return (null, "Username or email not found");
            }

            // Check if user is active
            if (!user.IsActive)
            {
                return (null, "Account has been deactivated. Please contact administrator");
            }

            // Verify password
            if (!VerifyPassword(request.Password, user.PasswordHash))
            {
                return (null, "Incorrect password");
            }

            // Get role name
            var role = await _context.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.RoleId == user.RoleId);
            var roleName = role?.RoleName ?? "Unknown";

            // Generate JWT token
            var token = GenerateJwtToken(user.UserId, user.Username, roleName);
            var expireMinutes = int.Parse(_configuration["Jwt:ExpireMinutes"] ?? "1440");
            var expiresAt = DateTime.UtcNow.AddMinutes(expireMinutes);

            // Check if first login
            var requirePasswordChange = user.IsFirstLogin;
            var message = requirePasswordChange 
                ? "Login successful. You must change your password before accessing other features."
                : "Login successful";

            var response = new LoginResponse
            {
                UserId = user.UserId,
                Username = user.Username,
                RoleName = roleName,
                Token = token,
                ExpiresAt = expiresAt,
                Message = message,
                RequirePasswordChange = requirePasswordChange
            };
            
            return (response, null);
        }

        /// <summary>
        /// Generate JWT token for authenticated user
        /// </summary>
        private string GenerateJwtToken(Guid userId, string username, string roleName)
        {
            var key = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured");
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var expireMinutes = int.Parse(_configuration["Jwt:ExpireMinutes"] ?? "1440");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, roleName)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Get all users (for Admin)
        /// </summary>
        public async Task<List<User>> GetAllUsers()
        {
            return await _context.Users.AsNoTracking().ToListAsync();
        }

        /// <summary>
        /// Change password for users
        /// Verifies old password before updating
        /// Sets IsFirstLogin = false
        /// Returns error message if validation fails
        /// </summary>
        public async Task<(User? User, string? ErrorMessage)> ChangePassword(Guid userId, string oldPassword, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return (null, "User not found");
            }

            // Verify old password
            if (!VerifyPassword(oldPassword, user.PasswordHash))
            {
                return (null, "Incorrect old password");
            }

            // Check if new password is same as old
            if (VerifyPassword(newPassword, user.PasswordHash))
            {
                return (null, "New password cannot be the same as old password");
            }

            // Hash and update password
            user.PasswordHash = HashPassword(newPassword);
            user.IsFirstLogin = false;

            await _context.SaveChangesAsync();
            return (user, null);
        }

        /// <summary>
        /// Get all roles
        /// </summary>
        public async Task<List<Role>> GetAllRoles()
        {
            return await _context.Roles.AsNoTracking().ToListAsync();
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        public async Task<User?> GetUserById(Guid userId)
        {
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);
        }

        /// <summary>
        /// Create new user (Admin only)
        /// Uses default password from configuration
        /// Sets IsFirstLogin = true
        /// </summary>
        public async Task<User> CreateUser(string username, string? displayName, string? email, string? phoneNumber, Guid roleId)
        {
            // Check if username already exists
            if (await _context.Users.AnyAsync(u => u.Username.ToLower() == username.ToLower()))
            {
                throw new Exception("Username already exists");
            }

            // Verify role exists
            if (!await _context.Roles.AnyAsync(r => r.RoleId == roleId))
            {
                throw new Exception("Role not found");
            }

            // Get default password from configuration (REQUIRED - no fallback)
            var defaultPassword = _configuration["DefaultPassword"];
            if (string.IsNullOrWhiteSpace(defaultPassword))
            {
                throw new InvalidOperationException("DefaultPassword is not configured in appsettings");
            }
            
            Console.WriteLine("[DEBUG] DefaultPassword loaded successfully for new user creation");
            var passwordHash = HashPassword(defaultPassword);

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Username = username,
                PasswordHash = passwordHash,
                DisplayName = displayName,
                Email = email,
                PhoneNumber = phoneNumber,
                RoleId = roleId,
                IsActive = true,
                IsFirstLogin = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        /// <summary>
        /// Update user (Admin only)
        /// Prevents admin from disabling themselves
        /// </summary>
        public async Task<User?> UpdateUser(Guid userId, Guid currentUserId, string? displayName, string? email, string? phoneNumber, bool? isActive)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return null;

            // Prevent admin from disabling themselves
            if (userId == currentUserId && isActive.HasValue && !isActive.Value)
            {
                throw new Exception("You cannot disable your own account");
            }

            if (displayName != null) user.DisplayName = displayName;
            if (email != null) user.Email = email;
            if (phoneNumber != null) user.PhoneNumber = phoneNumber;
            if (isActive.HasValue) user.IsActive = isActive.Value;

            await _context.SaveChangesAsync();
            return user;
        }

        /// <summary>
        /// Update profile (Self-update only)
        /// User can update their own profile information
        /// Requires password to be changed first (IsFirstLogin = false)
        /// </summary>
        public async Task<User?> UpdateProfile(Guid userId, string? displayName, string? email, string? phoneNumber)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return null;

            // Enforce password change requirement
            if (user.IsFirstLogin)
            {
                throw new Exception("You must change your password before updating profile");
            }

            // Update only provided fields
            if (displayName != null) user.DisplayName = displayName;
            if (email != null) user.Email = email;
            if (phoneNumber != null) user.PhoneNumber = phoneNumber;

            await _context.SaveChangesAsync();
            return user;
        }

        /// <summary>
        /// Soft delete user (Admin only) - Sets IsActive to false
        /// Prevents admin from disabling themselves
        /// </summary>
        public async Task<bool> DisableUser(Guid userId, Guid currentUserId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return false;

            // Prevent admin from disabling themselves
            if (userId == currentUserId)
            {
                throw new Exception("You cannot disable your own account");
            }

            user.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Assign role to user (Admin only)
        /// Prevents admin from removing their own admin role
        /// </summary>
        public async Task<User?> AssignRole(Guid userId, Guid roleId, Guid currentUserId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return null;

            // Verify role exists
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId);
            if (role == null) return null;

            // Prevent admin from removing their own Admin role
            if (userId == currentUserId)
            {
                var currentRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == user.RoleId);
                if (currentRole?.RoleName == "Admin" && role.RoleName != "Admin")
                {
                    throw new Exception("You cannot remove your own Admin role");
                }
            }

            user.RoleId = roleId;
            await _context.SaveChangesAsync();
            return user;
        }

        /// <summary>
        /// Get role by ID
        /// </summary>
        public async Task<Role?> GetRoleById(Guid roleId)
        {
            return await _context.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.RoleId == roleId);
        }

        /// <summary>
        /// Create new role (Admin only)
        /// </summary>
        public async Task<Role> CreateRole(string roleName)
        {
            // Check if role name already exists
            if (await _context.Roles.AnyAsync(r => r.RoleName.ToLower() == roleName.ToLower()))
            {
                throw new Exception("Role name already exists");
            }

            var role = new Role
            {
                RoleId = Guid.NewGuid(),
                RoleName = roleName
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            return role;
        }

        /// <summary>
        /// Update role (Admin only)
        /// </summary>
        public async Task<Role?> UpdateRole(Guid roleId, string roleName)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId);
            if (role == null) return null;

            // Check if new role name already exists (excluding current role)
            if (await _context.Roles.AnyAsync(r => r.RoleId != roleId && r.RoleName.ToLower() == roleName.ToLower()))
            {
                throw new Exception("Role name already exists");
            }

            role.RoleName = roleName;
            await _context.SaveChangesAsync();
            return role;
        }

        /// <summary>
        /// Delete role (Admin only)
        /// </summary>
        public async Task<bool> DeleteRole(Guid roleId)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId);
            if (role == null) return false;

            // Protect system roles from deletion
            var systemRoles = new[] { "admin", "manager", "reviewer", "annotator" };
            if (systemRoles.Contains(role.RoleName.ToLower()))
            {
                throw new Exception("Cannot delete system role");
            }

            // Check if any users have this role
            if (await _context.Users.AnyAsync(u => u.RoleId == roleId))
            {
                throw new Exception("Cannot delete role because users are assigned to it");
            }

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
