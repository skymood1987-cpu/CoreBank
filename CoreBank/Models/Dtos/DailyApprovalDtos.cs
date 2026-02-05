// Models/Dtos/DailyApprovalDtos.cs
using System.ComponentModel.DataAnnotations;

namespace MinCoreBank.Models.Dtos
{
    public class DailyBranchApprovalRequest
    {
        [Required]
        public string BranchId { get; set; } = string.Empty;

        [Required]
        public string ApprovedBy { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;
    }

    public class BranchApprovalStatusRequest
    {
        [Required]
        public string BranchId { get; set; } = string.Empty;
    }

    public class DailyApprovalResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal TotalCredit { get; set; }
        public decimal TotalDebit { get; set; }
        public bool IsBalanced { get; set; }
        public bool IsLocked { get; set; }
    }

    public class ApprovalStatusResponse
    {
        public bool IsApprovedToday { get; set; }
        public bool IsLocked { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal TotalDebit { get; set; }
        public bool IsBalanced { get; set; }
    }
}