// Models/SecurityModels.cs
namespace MinCoreBank.Models
{
    public class FailedLoginResult
    {
        public int FailedAttempts { get; set; }
        public bool AccountLocked { get; set; }
        public DateTime? LockedUntil { get; set; }
    }

    public class AdminResetPasswordDto
    {
        public string UserId { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}