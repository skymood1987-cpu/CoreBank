// IUserService.cs
using MinCoreBank.Models;
using MinCoreBank.Utilities;

public interface IUserService
{
    Task<ApiResponse<Users>> CreateUser(UserCreateDto userDto, string currentUserId);
    Task<ApiResponse<bool>> DeleteUser(string userId, string currentUserId);
    Task<ApiResponse<Users>> UpdateUser(string userId, UserUpdateDto userDto, string currentUserId);

    // FIXED: This should just be Task without ApiResponse
    Task UpdateLastLogin(string userId);

    Task<ApiResponse<Users>> UpdateUserstatus(string userId, UserUpdateStatusDto userDto, string currentUserId);
    Task<ApiResponse<IEnumerable<Users>>> GetAllUsers();
    Task<ApiResponse<Users>> GetUserById(string userId);

    // NEW: Get user by username
    Task<ApiResponse<Users>> GetUserByUsername(string username);

    // New methods for security features
    Task<FailedLoginResult> HandleFailedLogin(string userId);
    Task ResetFailedLoginAttempts(string userId);
    Task RequirePasswordChange(string userId);
    Task<ApiResponse<bool>> ChangePassword(string userId, string oldPassword, string newPassword);
    Task<ApiResponse<bool>> AdminResetPassword(string userId, string newPassword, string adminId);
    Task<ApiResponse<IEnumerable<Users>>> GetUsersWithExpiringPasswords(int daysUntilExpiry = 7);
    Task UnlockUserAccount(string userId, string adminId);
}