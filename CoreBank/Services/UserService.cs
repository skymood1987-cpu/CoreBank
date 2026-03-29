using Microsoft.Data.SqlClient;
using MinCoreBank.Models;
using System.Data;
using Dapper;
using MinCoreBank.Utilities;
using Microsoft.Extensions.Logging;

namespace MinCoreBank.Services
{
    public class UserService : IUserService
    {
        private readonly string _connectionString;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ILogger<UserService> _logger;

        public UserService(IConfiguration configuration, IPasswordHasher passwordHasher, ILogger<UserService> logger)
        {
            _connectionString = configuration.GetConnectionString("SecureConnection");
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        // FIXED: This returns just Task, not Task<ApiResponse>
        public async Task UpdateLastLogin(string userId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    "UPDATE users SET last_login = @LastLogin WHERE id = @UserId",
                    new { LastLogin = DateTime.Now, UserId = userId });

                _logger.LogDebug($"Updated last login for user {userId}");
            }
        }

        public async Task<FailedLoginResult> HandleFailedLogin(string userId)
        {
            try
            {
                _logger.LogInformation($"HandleFailedLogin called for user: {userId}");

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Get current failed attempts
                    var currentAttempts = await connection.ExecuteScalarAsync<int?>(
                        "SELECT failed_login_attempts FROM users WHERE id = @UserId",
                        new { UserId = userId });

                    _logger.LogInformation($"Current failed attempts for {userId}: {currentAttempts ?? 0}");

                    var newAttempts = (currentAttempts ?? 0) + 1;
                    _logger.LogInformation($"New failed attempts count: {newAttempts}");

                    if (newAttempts >= 5)
                    {
                        // Lock account for 15 minutes
                        var lockUntil = DateTime.Now.AddMinutes(15);

                        _logger.LogInformation($"Locking account {userId} until {lockUntil}");

                        var affectedRows = await connection.ExecuteAsync(
                            @"UPDATE users 
                      SET failed_login_attempts = @Attempts,
                          account_locked_until = @LockedUntil,
                          lock_reason = 'Too many failed attempts',
                          updated_at = @UpdatedAt
                      WHERE id = @UserId",
                            new
                            {
                                Attempts = newAttempts,
                                LockedUntil = lockUntil,
                                UpdatedAt = DateTime.Now,
                                UserId = userId
                            });

                        _logger.LogInformation($"Update executed. Rows affected: {affectedRows}");

                        return new FailedLoginResult
                        {
                            FailedAttempts = newAttempts,
                            AccountLocked = true,
                            LockedUntil = lockUntil
                        };
                    }
                    else
                    {
                        _logger.LogInformation($"Updating failed attempts to {newAttempts} for {userId}");

                        var affectedRows = await connection.ExecuteAsync(
                            @"UPDATE users 
                      SET failed_login_attempts = @Attempts,
                          updated_at = @UpdatedAt
                      WHERE id = @UserId",
                            new
                            {
                                Attempts = newAttempts,
                                UpdatedAt = DateTime.Now,
                                UserId = userId
                            });

                        _logger.LogInformation($"Update executed. Rows affected: {affectedRows}");

                        return new FailedLoginResult
                        {
                            FailedAttempts = newAttempts,
                            AccountLocked = false,
                            LockedUntil = null
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in HandleFailedLogin for user {userId}");
                return new FailedLoginResult
                {
                    FailedAttempts = 0,
                    AccountLocked = false,
                    LockedUntil = null
                };
            }
        }

        public async Task<ApiResponse<Users>> GetUserByUsername(string username)
        {
            _logger.LogInformation($"GetUserByUsername called for username: {username}");

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var user = await connection.QueryFirstOrDefaultAsync<Users>(
                        @"SELECT 
                    id AS Id,
                    name_ar AS Name_ar,
                    name_en AS Name_en,
                    role AS Role,
                    branch_id AS BranchId,
                    password_hash AS password_hash,
                    last_login AS LastLogin,
                    status AS Status,
                    created_at AS CreatedAt,
                    updated_at AS UpdatedAt,
                    updated_by AS UpdatedBy,
                    failed_login_attempts AS FailedLoginAttempts,
                    account_locked_until AS AccountLockedUntil,
                    last_password_change AS LastPasswordChange,
                    must_change_password AS MustChangePassword,
                    lock_reason AS LockReason
                FROM users WHERE name_en = @Username",
                        new { Username = username });

                    if (user == null)
                    {
                        _logger.LogWarning($"User not found: {username}");
                        return new ApiResponse<Users> { Success = false, Message = "User not found" };
                    }

                    _logger.LogInformation($"User found: ID={user.Id}, Name={user.Name_en}, Status={user.Status}");
                    return new ApiResponse<Users> { Success = true, Data = user };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetUserByUsername for username: {username}");
                return new ApiResponse<Users> { Success = false, Message = "Database error" };
            }
        }
        // FIXED: This returns just Task, not Task<ApiResponse>
        public async Task ResetFailedLoginAttempts(string userId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    // Use server time instead of Baghdad time
                    var updatedAt = DateTime.Now;  // Changed to server time

                    await connection.ExecuteAsync(
                        @"UPDATE users 
                  SET failed_login_attempts = 0,
                      account_locked_until = NULL,
                      lock_reason = NULL,
                      updated_at = @UpdatedAt
                  WHERE id = @UserId",
                        new
                        {
                            UpdatedAt = updatedAt,
                            UserId = userId
                        });

                    _logger.LogInformation($"Reset failed login attempts for user {userId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error resetting failed attempts for user {userId}");
            }
        }
        // FIXED: This returns just Task, not Task<ApiResponse>
        public async Task RequirePasswordChange(string userId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    @"UPDATE users 
                      SET must_change_password = 1,
                          updated_at = @UpdatedAt
                      WHERE id = @UserId",
                    new
                    {
                        UpdatedAt = DateTime.Now,
                        UserId = userId
                    });

                _logger.LogInformation($"Password change required for user {userId}");
            }
        }

        public async Task<ApiResponse<bool>> ChangePassword(string userId, string oldPassword, string newPassword)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Get current user info
                var user = await connection.QueryFirstOrDefaultAsync<Users>(
                    "SELECT * FROM users WHERE id = @UserId AND status = 'active'",
                    new { UserId = userId });

                if (user == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "User not found or inactive"
                    };
                }

                // Check if account is locked
                if (user.AccountLockedUntil.HasValue && user.AccountLockedUntil.Value > DateTime.Now)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = $"Account is locked until {user.AccountLockedUntil.Value:yyyy-MM-dd HH:mm:ss}"
                    };
                }

                // Verify old password
                if (!_passwordHasher.VerifyPassword(user.password_hash, oldPassword))
                {
                    // Increment failed attempts
                    var lockResult = await HandleFailedLogin(userId);

                    if (lockResult.AccountLocked)
                    {
                        return new ApiResponse<bool>
                        {
                            Success = false,
                            Message = $"Too many failed attempts. Account locked until {lockResult.LockedUntil:yyyy-MM-dd HH:mm:ss}"
                        };
                    }

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = $"Current password is incorrect. {3 - lockResult.FailedAttempts} attempts remaining."
                    };
                }

                // Password validation rules
                if (newPassword.Length < 8)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Password must be at least 8 characters long"
                    };
                }

                // Check if new password is same as old password
                if (_passwordHasher.VerifyPassword(user.password_hash, newPassword))
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "New password cannot be the same as current password"
                    };
                }

                // Hash new password
                var newHash = _passwordHasher.HashPassword(newPassword);

                // Update password and reset flags
                await connection.ExecuteAsync(
                    @"UPDATE users 
                      SET password_hash = @NewHash,
                          last_password_change = @ChangeDate,
                          must_change_password = 0,
                          failed_login_attempts = 0,
                          account_locked_until = NULL,
                          lock_reason = NULL,
                          updated_at = @UpdatedAt
                      WHERE id = @UserId",
                    new
                    {
                        NewHash = newHash,
                        ChangeDate = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        UserId = userId
                    });

                _logger.LogInformation($"Password changed successfully for user {userId}");

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Password changed successfully",
                    Data = true
                };
            }
        }

        public async Task<ApiResponse<bool>> AdminResetPassword(string userId, string newPassword, string adminId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Check if admin exists and is active
                var admin = await connection.QueryFirstOrDefaultAsync<Users>(
                    "SELECT * FROM users WHERE id = @AdminId AND role = 'admin' AND status = 'active'",
                    new { AdminId = adminId });

                if (admin == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Admin not found or unauthorized"
                    };
                }

                // Check if user exists
                var user = await connection.QueryFirstOrDefaultAsync<Users>(
                    "SELECT * FROM users WHERE id = @UserId",
                    new { UserId = userId });

                if (user == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                // Password validation
                if (newPassword.Length < 8)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Password must be at least 8 characters long"
                    };
                }

                // Hash new password
                var newHash = _passwordHasher.HashPassword(newPassword);

                // Update password and force change on next login
                await connection.ExecuteAsync(
                    @"UPDATE users 
                      SET password_hash = @NewHash,
                          last_password_change = @ChangeDate,
                          must_change_password = 1,
                          failed_login_attempts = 0,
                          account_locked_until = NULL,
                          lock_reason = NULL,
                          updated_at = @UpdatedAt,
                          updated_by = @AdminId
                      WHERE id = @UserId",
                    new
                    {
                        NewHash = newHash,
                        ChangeDate = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        AdminId = adminId,
                        UserId = userId
                    });

                _logger.LogInformation($"Password reset by admin {adminId} for user {userId}");

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Password reset successfully. User must change password on next login.",
                    Data = true
                };
            }
        }

        public async Task<ApiResponse<Users>> CreateUser(UserCreateDto userDto, string currentUserId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Check if username already exists
                var existingUser = await connection.QueryFirstOrDefaultAsync<Users>(
                    "SELECT * FROM users WHERE name_en = @Username",
                    new { Username = userDto.NameEn });

                if (existingUser != null)
                {
                    return new ApiResponse<Users>
                    {
                        Success = false,
                        Message = "Username already exists"
                    };
                }

                // Validate role
                var validRoles = new[] { "teller", "manager", "admin", "auditor" };
                if (!validRoles.Contains(userDto.Role.ToLower()))
                {
                    return new ApiResponse<Users>
                    {
                        Success = false,
                        Message = "Invalid role. Must be: teller, manager, admin, or auditor"
                    };
                }

                // Validate password
                if (userDto.Password.Length < 8)
                {
                    return new ApiResponse<Users>
                    {
                        Success = false,
                        Message = "Password must be at least 8 characters long"
                    };
                }

                var baghdadTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Baghdad");
                var baghdadTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Now, baghdadTimeZone);
                var userId = $"USER-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4)}";
                var passwordHash = _passwordHasher.HashPassword(userDto.Password);
                var now = DateTime.Now;

                var user = new Users
                {
                    Id = userId,
                    Name_ar = userDto.NameAr,
                    Name_en = userDto.NameEn,
                    Role = userDto.Role.ToLower(),
                    BranchId = userDto.BranchId,
                    password_hash = passwordHash,
                    Status = "active",
                    CreatedAt = baghdadTime,
                    UpdatedAt = baghdadTime,
                    UpdatedBy = currentUserId,
                    FailedLoginAttempts = 0,
                    LastPasswordChange = now,
                    MustChangePassword = false,
                    AccountLockedUntil = null,
                    LockReason = null,
                    LastLogin = null
                };

                await connection.ExecuteAsync(
    @"INSERT INTO users 
      (id, name_ar, name_en, role, branch_id, password_hash, status, 
       created_at, updated_at, updated_by, failed_login_attempts, 
       last_password_change, must_change_password, account_locked_until, lock_reason) 
      VALUES (@Id, @Name_ar, @Name_en, @Role, @BranchId, @password_hash, @Status, 
              @CreatedAt, @UpdatedAt, @UpdatedBy, @FailedLoginAttempts, 
              @LastPasswordChange, @MustChangePassword, @AccountLockedUntil, @LockReason)",
    user);

                _logger.LogInformation($"User created: {userId} by {currentUserId}");

                return new ApiResponse<Users>
                {
                    Success = true,
                    Message = "User created successfully",
                    Data = user
                };
            }
        }

        public async Task<ApiResponse<bool>> DeleteUser(string userId, string currentUserId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var affectedRows = await connection.ExecuteAsync(
                    @"UPDATE users 
                      SET status = 'disabled', 
                          updated_at = @UpdatedAt, 
                          updated_by = @UpdatedBy
                      WHERE id = @UserId",
                    new
                    {
                        UserId = userId,
                        UpdatedAt = DateTime.Now,
                        UpdatedBy = currentUserId
                    });

                if (affectedRows == 0)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                _logger.LogInformation($"User disabled: {userId} by {currentUserId}");

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "User disabled successfully",
                    Data = true
                };
            }
        }

        public async Task<ApiResponse<Users>> UpdateUser(string userId, UserUpdateDto userDto, string currentUserId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Validate status if provided
                if (!string.IsNullOrEmpty(userDto.Status))
                {
                    var validStatuses = new[] { "active", "suspended", "disabled" };
                    if (!validStatuses.Contains(userDto.Status.ToLower()))
                    {
                        return new ApiResponse<Users>
                        {
                            Success = false,
                            Message = "Invalid status"
                        };
                    }
                }

                // Validate role if provided
                if (!string.IsNullOrEmpty(userDto.Role))
                {
                    var validRoles = new[] { "teller", "manager", "admin", "auditor" };
                    if (!validRoles.Contains(userDto.Role.ToLower()))
                    {
                        return new ApiResponse<Users>
                        {
                            Success = false,
                            Message = "Invalid role"
                        };
                    }
                }

                var now = DateTime.Now;

                var user = await connection.QueryFirstOrDefaultAsync<Users>(
                    @"UPDATE users 
                      SET 
                        name_ar = COALESCE(@NameAr, name_ar),
                        name_en = COALESCE(@NameEn, name_en),
                        role = COALESCE(@Role, role),
                        status = COALESCE(@Status, status),
                        updated_at = @UpdatedAt,
                        updated_by = @UpdatedBy
                      WHERE id = @UserId
                      SELECT * FROM users WHERE id = @UserId",
                    new
                    {
                        UserId = userId,
                        NameAr = userDto.NameAr,
                        NameEn = userDto.NameEn,
                        Role = userDto.Role?.ToLower(),
                        Status = userDto.Status?.ToLower(),
                        UpdatedAt = now,
                        UpdatedBy = currentUserId
                    });

                if (user == null)
                {
                    return new ApiResponse<Users>
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                _logger.LogInformation($"User updated: {userId} by {currentUserId}");

                return new ApiResponse<Users>
                {
                    Success = true,
                    Message = "User updated successfully",
                    Data = user
                };
            }
        }

        public async Task<ApiResponse<Users>> UpdateUserstatus(string userId, UserUpdateStatusDto userDto, string currentUserId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Validate status if provided
                if (!string.IsNullOrEmpty(userDto.Status))
                {
                    var validStatuses = new[] { "active", "suspended", "disabled" };
                    if (!validStatuses.Contains(userDto.Status.ToLower()))
                    {
                        return new ApiResponse<Users>
                        {
                            Success = false,
                            Message = "Invalid status"
                        };
                    }
                }

                var now = DateTime.Now;

                var user = await connection.QueryFirstOrDefaultAsync<Users>(
                    @"UPDATE users 
                      SET 
                        status = COALESCE(@Status, status),
                        updated_at = @UpdatedAt,
                        updated_by = @UpdatedBy
                      WHERE id = @UserId
                      SELECT * FROM users WHERE id = @UserId",
                    new
                    {
                        UserId = userId,
                        Status = userDto.Status?.ToLower(),
                        UpdatedAt = now,
                        UpdatedBy = currentUserId
                    });

                if (user == null)
                {
                    return new ApiResponse<Users>
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                _logger.LogInformation($"User status updated: {userId} to {userDto.Status} by {currentUserId}");

                return new ApiResponse<Users>
                {
                    Success = true,
                    Message = "User updated successfully",
                    Data = user
                };
            }
        }

        public async Task<ApiResponse<IEnumerable<Users>>> GetAllUsers()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var users = await connection.QueryAsync<Users>(@"
            SELECT 
                id AS Id,
                name_ar AS Name_ar,
                name_en AS Name_en,
                role AS Role,
                branch_id AS BranchId,  
                password_hash AS password_hash,
                last_login AS LastLogin,
                status AS Status,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt,
                updated_by AS UpdatedBy,
                failed_login_attempts AS FailedLoginAttempts,
                account_locked_until AS AccountLockedUntil,
                last_password_change AS LastPasswordChange,
                must_change_password AS MustChangePassword,
                lock_reason AS LockReason
            FROM users 
            ORDER BY created_at DESC");

                return new ApiResponse<IEnumerable<Users>>
                {
                    Success = true,
                    Data = users
                };
            }
        }

        public async Task<ApiResponse<Users>> GetUserById(string userId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var user = await connection.QueryFirstOrDefaultAsync<Users>(
                    @"SELECT 
                        id AS Id,
                        name_ar AS Name_ar,
                        name_en AS Name_en,
                        role AS Role,
                        branch_id AS BranchId,  
                        password_hash AS password_hash,
                        last_login AS LastLogin,
                        status AS Status,
                        created_at AS CreatedAt,
                        updated_at AS UpdatedAt,
                        updated_by AS UpdatedBy,
                        failed_login_attempts AS FailedLoginAttempts,
                        account_locked_until AS AccountLockedUntil,
                        last_password_change AS LastPasswordChange,
                        must_change_password AS MustChangePassword,
                        lock_reason AS LockReason
                    FROM users 
                    WHERE id = @UserId",
                    new { UserId = userId });

                if (user == null)
                {
                    return new ApiResponse<Users>
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                return new ApiResponse<Users>
                {
                    Success = true,
                    Message = "User retrieved successfully",
                    Data = user
                };
            }
        }

        public async Task<ApiResponse<IEnumerable<Users>>> GetUsersWithExpiringPasswords(int daysUntilExpiry = 7)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                const int passwordExpiryDays = 30;
                var warningDate = DateTime.Now.AddDays(-(passwordExpiryDays - daysUntilExpiry));

                var users = await connection.QueryAsync<Users>(@"
            SELECT 
                id AS Id,
                name_ar AS Name_ar,
                name_en AS Name_en,
                role AS Role,
                branch_id AS BranchId,  
                last_login AS LastLogin,
                status AS Status,
                last_password_change AS LastPasswordChange,
                must_change_password AS MustChangePassword
            FROM users 
            WHERE status = 'active' 
              AND must_change_password = 0
              AND last_password_change <= @WarningDate
            ORDER BY last_password_change ASC",
                    new { WarningDate = warningDate });

                return new ApiResponse<IEnumerable<Users>>
                {
                    Success = true,
                    Data = users
                };
            }
        }

        // FIXED: This returns just Task, not Task<ApiResponse>
        public async Task UnlockUserAccount(string userId, string adminId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    @"UPDATE users 
                      SET failed_login_attempts = 0,
                          account_locked_until = NULL,
                          lock_reason = NULL,
                          updated_at = @UpdatedAt,
                          updated_by = @AdminId
                      WHERE id = @UserId",
                    new
                    {
                        UpdatedAt = DateTime.Now,
                        AdminId = adminId,
                        UserId = userId
                    });

                _logger.LogInformation($"Account unlocked for user {userId} by admin {adminId}");
            }
        }
    }

    public interface IPasswordHasher
    {
        string HashPassword(string password);
        bool VerifyPassword(string hash, string password);
    }

    public class PasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string hash, string password)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}
