// Repositories/IDailyBranchApprovalRepository.cs
using MinCoreBank.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MinCoreBank.Repositories
{
    public interface IDailyBranchApprovalRepository
    {
        Task<bool> HasBranchBeenApprovedTodayAsync(string branchId);
        Task<DailyBranchApproval> RecordDailyApprovalAsync(DailyBranchApproval approval);

        // Overload for specific date
        Task<BranchDailyBalanceResult> GetBranchDailyBalanceAsync(string branchId, DateTime date);

        // Method for today
        Task<BranchDailyBalanceResult> GetBranchDailyBalanceAsync(string branchId);

        Task<DailyBranchApproval> GetTodayApprovalAsync(string branchId);
        Task<bool> IsDateLockedAsync(string branchId, DateTime date);
        Task<List<DateTime>> GetLockedDatesAsync(string branchId);
    }

    // Add this result class to avoid deconstruction issues
    public class BranchDailyBalanceResult
    {
        public decimal TotalCredit { get; set; }
        public decimal TotalDebit { get; set; }
        public bool IsBalanced { get; set; }
    }
}