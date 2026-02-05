// Models/ViewModels/DailyBranchApprovalViewModel.cs
using MinCoreBank.Models;

namespace MinCoreBank.Models.ViewModels
{
    public class DailyBranchApprovalViewModel
    {
        public string BranchId { get; set; } = string.Empty;
        public DateTime Today { get; set; }
        public bool IsApprovedToday { get; set; }
        public DailyBranchApproval? TodayApproval { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal TotalDebit { get; set; }
        public bool IsBalanced { get; set; }
        public decimal Difference => TotalCredit - TotalDebit;
    }

    public class DailyApprovalRequest
    {
        public string? Notes { get; set; }
    }
}