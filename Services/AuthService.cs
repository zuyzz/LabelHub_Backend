using DataLabel_Project_BE.DTOs.Auth;
using DataLabel_Project_BE.Models;
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
    /// MOCK DATA IMPLEMENTATION:
    /// - Uses IN-MEMORY storage (List&lt;User&gt;, List&lt;Role&gt;)
    /// - Data resets on application restart
    /// - Seed users: admin (ready), reviewer_demo (requires password change)
    /// - Default password for new users: configured in appsettings.json
    /// 
    /// Implements password hashing using SHA256 and JWT token generation
    /// </summary>
    public class AuthService
    {
        private readonly IConfiguration _configuration;
        private static readonly List<User> _users = new List<User>();
        private static readonly List<Role> _roles = new List<Role>();
        private static bool _isInitialized = false;
        private static readonly object _lock = new object();

        public AuthService(IConfiguration configuration)
        {
            _configuration = configuration;
            
            // Thread-safe initialization
            lock (_lock)
            {
                if (!_isInitialized)
                {
                    InitializeSeedData();
                    _isInitialized = true;
                }
            }
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
        /// Uses IN-MEMORY mock data
        /// Validates username/email, password, and active status
        /// </summary>
        public LoginResponse? Login(LoginRequest request)
        {
            // Find user by username or email (case-insensitive)
            var user = _users.FirstOrDefault(u =>
                u.Username.Equals(request.UsernameOrEmail, StringComparison.OrdinalIgnoreCase) ||
                (u.Email != null && u.Email.Equals(request.UsernameOrEmail, StringComparison.OrdinalIgnoreCase)));

            // Check if user exists
            if (user == null)
            {
                return null; // User not found
            }

            // Check if user is active
            if (!user.IsActive)
            {
                return null; // User is deactivated
            }

            // Verify password
            if (!VerifyPassword(request.Password, user.PasswordHash))
            {
                return null; // Invalid password
            }

            // Get role name
            var role = _roles.FirstOrDefault(r => r.RoleId == user.RoleId);
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

            return new LoginResponse
            {
                UserId = user.UserId,
                Username = user.Username,
                RoleName = roleName,
                Token = token,
                ExpiresAt = expiresAt,
                Message = message,
                RequirePasswordChange = requirePasswordChange
            };
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
        public List<User> GetAllUsers()
        {
            return _users.ToList();
        }

        /// <summary>
        /// Change password for first login users
        /// Verifies old password before updating
        /// Sets IsFirstLogin = false
        /// </summary>
        public User? ChangePassword(Guid userId, string oldPassword, string newPassword)
        {
            var user = _users.FirstOrDefault(u => u.UserId == userId);
            if (user == null) return null;

            // Verify old password
            if (!VerifyPassword(oldPassword, user.PasswordHash))
            {
                return null; // Old password is incorrect
            }

            // Hash and update password
            user.PasswordHash = HashPassword(newPassword);
            user.IsFirstLogin = false;

            return user;
        }

        /// <summary>
        /// Get all roles
        /// </summary>
        public List<Role> GetAllRoles()
        {
            return _roles.ToList();
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        public User? GetUserById(Guid userId)
        {
            return _users.FirstOrDefault(u => u.UserId == userId);
        }

        /// <summary>
        /// Create new user (Admin only)
        /// Uses default password from configuration
        /// Sets IsFirstLogin = true
        /// </summary>
        public User CreateUser(string username, string? displayName, string? email, string? phoneNumber, Guid roleId)
        {
            // Check if username already exists
            if (_users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
            {
                throw new Exception("Username already exists");
            }

            // Verify role exists
            if (!_roles.Any(r => r.RoleId == roleId))
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

            _users.Add(user);
            return user;
        }

        /// <summary>
        /// Update user (Admin only)
        /// Prevents admin from disabling themselves
        /// </summary>
        public User? UpdateUser(Guid userId, Guid currentUserId, string? displayName, string? email, string? phoneNumber, bool? isActive)
        {
            var user = _users.FirstOrDefault(u => u.UserId == userId);
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

            return user;
        }

        /// <summary>
        /// Update profile (Self-update only)
        /// User can update their own profile information
        /// Requires password to be changed first (IsFirstLogin = false)
        /// </summary>
        public User? UpdateProfile(Guid userId, string? displayName, string? email, string? phoneNumber)
        {
            var user = _users.FirstOrDefault(u => u.UserId == userId);
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

            return user;
        }

        /// <summary>
        /// Soft delete user (Admin only) - Sets IsActive to false
        /// Prevents admin from disabling themselves
        /// </summary>
        public bool DisableUser(Guid userId, Guid currentUserId)
        {
            var user = _users.FirstOrDefault(u => u.UserId == userId);
            if (user == null) return false;

            // Prevent admin from disabling themselves
            if (userId == currentUserId)
            {
                throw new Exception("You cannot disable your own account");
            }

            user.IsActive = false;
            return true;
        }

        /// <summary>
        /// Assign role to user (Admin only)
        /// Prevents admin from removing their own admin role
        /// </summary>
        public User? AssignRole(Guid userId, Guid roleId, Guid currentUserId)
        {
            var user = _users.FirstOrDefault(u => u.UserId == userId);
            if (user == null) return null;

            // Verify role exists
            var role = _roles.FirstOrDefault(r => r.RoleId == roleId);
            if (role == null) return null;

            // Prevent admin from removing their own Admin role
            if (userId == currentUserId)
            {
                var currentRole = _roles.FirstOrDefault(r => r.RoleId == user.RoleId);
                if (currentRole?.RoleName == "Admin" && role.RoleName != "Admin")
                {
                    throw new Exception("You cannot remove your own Admin role");
                }
            }

            user.RoleId = roleId;
            return user;
        }

        /// <summary>
        /// Get role by ID
        /// </summary>
        public Role? GetRoleById(Guid roleId)
        {
            return _roles.FirstOrDefault(r => r.RoleId == roleId);
        }

        /// <summary>
        /// Create new role (Admin only)
        /// </summary>
        public Role CreateRole(string roleName)
        {
            // Check if role name already exists
            if (_roles.Any(r => r.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new Exception("Role name already exists");
            }

            var role = new Role
            {
                RoleId = Guid.NewGuid(),
                RoleName = roleName
            };

            _roles.Add(role);
            return role;
        }

        /// <summary>
        /// Update role (Admin only)
        /// </summary>
        public Role? UpdateRole(Guid roleId, string roleName)
        {
            var role = _roles.FirstOrDefault(r => r.RoleId == roleId);
            if (role == null) return null;

            // Check if new role name already exists (excluding current role)
            if (_roles.Any(r => r.RoleId != roleId && r.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new Exception("Role name already exists");
            }

            role.RoleName = roleName;
            return role;
        }

        /// <summary>
        /// Delete role (Admin only)
        /// </summary>
        public bool DeleteRole(Guid roleId)
        {
            var role = _roles.FirstOrDefault(r => r.RoleId == roleId);
            if (role == null) return false;

            // Check if any users have this role
            if (_users.Any(u => u.RoleId == roleId))
            {
                throw new Exception("Cannot delete role because users are assigned to it");
            }

            _roles.Remove(role);
            return true;
        }

        /// <summary>
        /// Initialize seed data with FIXED GUIDs for testing
        /// 
        /// MOCK DATA BEHAVIOR:
        /// - Data is stored IN-MEMORY only
        /// - Restarting the app resets all users except these seeded ones
        /// - Default password for demo users: see "DefaultPassword" in appsettings.json
        /// 
        /// Creates:
        /// - 4 fixed roles: Admin, Manager, Reviewer, Annotator
        /// - 1 admin account (ready to use)
        /// - 1 demo reviewer (requires password change on first login)
        /// </summary>
        private void InitializeSeedData()
        {
            // Create roles with FIXED GUIDs
            var adminRoleId = new Guid("11111111-1111-1111-1111-111111111111");
            var managerRoleId = new Guid("22222222-2222-2222-2222-222222222222");
            var reviewerRoleId = new Guid("33333333-3333-3333-3333-333333333333");
            var annotatorRoleId = new Guid("44444444-4444-4444-4444-444444444444");

            _roles.Add(new Role { RoleId = adminRoleId, RoleName = "Admin" });
            _roles.Add(new Role { RoleId = managerRoleId, RoleName = "Manager" });
            _roles.Add(new Role { RoleId = reviewerRoleId, RoleName = "Reviewer" });
            _roles.Add(new Role { RoleId = annotatorRoleId, RoleName = "Annotator" });

            // Get default password from configuration
            var defaultPassword = _configuration["DefaultPassword"];
            if (string.IsNullOrWhiteSpace(defaultPassword))
            {
                throw new InvalidOperationException("DefaultPassword is not configured in appsettings");
            }
            
            Console.WriteLine("[DEBUG] DefaultPassword loaded successfully at startup");

            // Create admin user with FIXED GUID (ready to use, no password change required)
            var adminUserId = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
            var adminUser = new User
            {
                UserId = adminUserId,
                Username = "admin",
                Email = "admin@datalabel.com",
                PasswordHash = HashPassword("Admin@123"), // Admin has custom password
                DisplayName = "System Administrator",
                PhoneNumber = null,
                RoleId = adminRoleId,
                IsActive = true,
                IsFirstLogin = false, // Admin doesn't need to change password
                CreatedAt = DateTime.UtcNow
            };

            // Create demo reviewer user (for testing first-login flow)
            var reviewerUserId = new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
            var reviewerUser = new User
            {
                UserId = reviewerUserId,
                Username = "reviewer_demo",
                Email = "reviewer@datalabel.com",
                PasswordHash = HashPassword(defaultPassword), // Uses default password
                DisplayName = "Demo Reviewer",
                PhoneNumber = null,
                RoleId = reviewerRoleId,
                IsActive = true,
                IsFirstLogin = true, // Must change password on first login
                CreatedAt = DateTime.UtcNow
            };

            _users.Add(adminUser);
            _users.Add(reviewerUser);
        }
    }
}
