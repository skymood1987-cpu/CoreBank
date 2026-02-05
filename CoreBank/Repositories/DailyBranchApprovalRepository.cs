// Repositories/DailyBranchApprovalRepository.cs
using Microsoft.EntityFrameworkCore;
using MinCoreBank.Data;
using MinCoreBank.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinCoreBank.Repositories
{
    public class DailyBranchApprovalRepository : IDailyBranchApprovalRepository
    {
        private readonly AppDbContext _context;

        public DailyBranchApprovalRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> HasBranchBeenApprovedTodayAsync(string branchId)
        {
            var today = DateTime.Today;
            return await _context.DailyBranchApprovals
                .AnyAsync(a => a.BranchId == branchId && a.ApprovalDate.Date == today);
        }

        public async Task<DailyBranchApproval> RecordDailyApprovalAsync(DailyBranchApproval approval)
        {
            _context.DailyBranchApprovals.Add(approval);
            await _context.SaveChangesAsync();
            return approval;
        }

       
            public async Task<BranchDailyBalanceResult> GetBranchDailyBalanceAsync(string branchId, DateTime date)
        {
            // Calculate total of ALL debit_account values in the branch for SPECIFIC date
            var totalDebit = await _context.GlTransactions
                .Where(t => t.BranchId == branchId &&
                           t.Date.Value.Date == date.Date &&
                           t.Status != "reversed")
                .SumAsync(t => t.DebitAccount ?? 0);  // SUM the debit_account column!

            // Calculate total of ALL credit_account values in the branch for SPECIFIC date  
            var totalCredit = await _context.GlTransactions
                .Where(t => t.BranchId == branchId &&
                           t.Date.Value.Date == date.Date &&
                           t.Status != "reversed")
                .SumAsync(t => t.CreditAccount ?? 0);  // SUM the credit_account column!

            return new BranchDailyBalanceResult
            {
                TotalCredit = totalCredit,
                TotalDebit = totalDebit,
                IsBalanced = totalCredit == totalDebit
            };
        }
        public async Task<BranchDailyBalanceResult> GetBranchDailyBalanceAsync(string branchId)
        {
            return await GetBranchDailyBalanceAsync(branchId, DateTime.Today);
        }

        public async Task<DailyBranchApproval> GetTodayApprovalAsync(string branchId)
        {
            var today = DateTime.Today;
            return await _context.DailyBranchApprovals
                .FirstOrDefaultAsync(a => a.BranchId == branchId && a.ApprovalDate.Date == today);
        }

        public async Task<bool> IsDateLockedAsync(string branchId, DateTime date)
        {
            return await _context.DailyBranchApprovals
                .AnyAsync(a => a.BranchId == branchId &&
                              a.ApprovalDate.Date == date.Date &&
                              a.IsLocked);
        }

        public async Task<List<DateTime>> GetLockedDatesAsync(string branchId)
        {
            return await _context.DailyBranchApprovals
                .Where(a => a.BranchId == branchId && a.IsLocked)
                .Select(a => a.ApprovalDate.Date)
                .ToListAsync();
        }
    }
}