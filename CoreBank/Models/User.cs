namespace MinCoreBank.Models
{
    public class Users
    {

        public string Id { get; set; }
        public string Name_ar { get; set; }
        public string Name_en { get; set; }
        public string Role { get; set; }
        public string BranchId { get; set; }
        public string password_hash { get; set; }
        public DateTime? LastLogin { get; set; }
        public string Status { get; set; } = "active";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string UpdatedBy { get; set; }
        public Branch Branch { get; set; }    // Navigation property

       
        public int FailedLoginAttempts { get; set; }
        public DateTime? AccountLockedUntil { get; set; }
        public DateTime LastPasswordChange { get; set; }
        public bool MustChangePassword { get; set; }
        public string? LockReason { get; set; }
    }

    public class UserCreateDto
    {
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string Role { get; set; }
        public string BranchId { get; set; }
        public string Password { get; set; }
    }

    public class UserUpdateDto
    {
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
    }


    public class UserUpdateStatusDto
    {
       
        public string Status { get; set; }
    }
}