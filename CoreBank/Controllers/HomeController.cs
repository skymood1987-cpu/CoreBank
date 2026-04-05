using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinCoreBank.Data;
using MinCoreBank.Models.ViewModels;

namespace MinCoreBank.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var branchId = User.FindFirst("BranchCode")?.Value ?? string.Empty;
            var today = DateTime.Today;

            var model = new HomeDashboardViewModel
            {
                BranchId = branchId,
                BusinessDate = today
            };

            if (!string.IsNullOrWhiteSpace(branchId))
            {
                var normalizedBranchId = branchId.Trim();
                var monthStart = new DateTime(today.Year, today.Month, 1);

                var todayTransactions = _context.GlTransactions
                    .AsNoTracking()
                    .Where(t =>
                        t.BranchId != null &&
                        t.BranchId.Trim() == normalizedBranchId &&
                        t.Date.HasValue &&
                        t.Date.Value.Date == today &&
                        t.Status == "completed");

                model.TotalDebitToday = await todayTransactions.SumAsync(t => t.DebitAccount ?? 0m);
                model.TotalCreditToday = await todayTransactions.SumAsync(t => t.CreditAccount ?? 0m);
                model.TransactionsCountToday = await todayTransactions
                    .Select(t => t.TransactionRef != null ? t.TransactionRef.Trim() : null)
                    .Where(r => !string.IsNullOrWhiteSpace(r))
                    .Distinct()
                    .CountAsync();
                model.DailyDifference = model.TotalCreditToday - model.TotalDebitToday;
                model.DailyMovementAmount = Math.Max(model.TotalDebitToday, model.TotalCreditToday);

                var branchAccountsQuery = _context.GeneralLedgerAccounts
                    .AsNoTracking()
                    .Where(a => a.BranchId != null && a.BranchId.Trim() == normalizedBranchId);

                model.AccountsCount = await branchAccountsQuery.CountAsync();
                model.HasBranchBalanceData = model.AccountsCount > 0;

                // Gross branch balance = sum of absolute balances (useful total exposure).
                model.BranchTotalBalance = await branchAccountsQuery
                    .Select(a => (decimal?)Math.Abs(a.Balance))
                    .SumAsync() ?? 0m;

                // Net branch balance = algebraic sum.
                model.BranchNetBalance = await branchAccountsQuery
                    .Select(a => (decimal?)a.Balance)
                    .SumAsync() ?? 0m;

                model.DailyMovementToBranchBalancePct = model.BranchTotalBalance > 0
                    ? (model.DailyMovementAmount / model.BranchTotalBalance) * 100m
                    : 0m;

                model.ReversedCountToday = await _context.GlTransactions
                    .AsNoTracking()
                    .Where(t =>
                        t.BranchId != null &&
                        t.BranchId.Trim() == normalizedBranchId &&
                        t.Date.HasValue &&
                        t.Date.Value.Date == today &&
                        t.Status == "reversed")
                    .CountAsync();

                var allTodayCount = model.TransactionsCountToday + model.ReversedCountToday;
                model.CompletionRateToday = allTodayCount > 0
                    ? (decimal)model.TransactionsCountToday * 100m / allTodayCount
                    : 0m;

                model.AvgTransactionValueToday = model.TransactionsCountToday > 0
                    ? model.DailyMovementAmount / model.TransactionsCountToday
                    : 0m;

                var workingDatesDesc = new List<DateTime>();
                for (var d = today; workingDatesDesc.Count < 7; d = d.AddDays(-1))
                {
                    if (d.DayOfWeek == DayOfWeek.Friday || d.DayOfWeek == DayOfWeek.Saturday)
                    {
                        continue;
                    }
                    workingDatesDesc.Add(d);
                }

                var workingDates = workingDatesDesc.OrderBy(d => d).ToList();
                var trendStart = workingDates.First();
                var trendData = await _context.GlTransactions
                    .AsNoTracking()
                    .Where(t =>
                        t.BranchId != null &&
                        t.BranchId.Trim() == normalizedBranchId &&
                        t.Date.HasValue &&
                        t.Date.Value.Date >= trendStart &&
                        t.Date.Value.Date <= today &&
                        t.Status == "completed")
                    .GroupBy(t => t.Date!.Value.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        Count = g
                            .Select(t => t.TransactionRef != null ? t.TransactionRef.Trim() : null)
                            .Where(r => !string.IsNullOrWhiteSpace(r))
                            .Distinct()
                            .Count()
                    })
                    .ToListAsync();

                model.DailyTransactionCounts = workingDates
                    .Select(d => new DailyTransactionCountPoint
                    {
                        Date = d,
                        Count = trendData.FirstOrDefault(x => x.Date == d)?.Count ?? 0
                    })
                    .ToList();

                model.MaxDailyTransactionCount = Math.Max(1, model.DailyTransactionCounts.Max(x => x.Count));

                var monthTransactions = _context.GlTransactions
                    .AsNoTracking()
                    .Where(t =>
                        t.BranchId != null &&
                        t.BranchId.Trim() == normalizedBranchId &&
                        t.Date.HasValue &&
                        t.Date.Value.Date >= monthStart &&
                        t.Date.Value.Date <= today &&
                        t.Status == "completed");

                model.MonthTotalDebit = await monthTransactions.SumAsync(t => t.DebitAccount ?? 0m);
                model.MonthTotalCredit = await monthTransactions.SumAsync(t => t.CreditAccount ?? 0m);
                model.MonthMovementTotal = model.MonthTotalDebit + model.MonthTotalCredit;
                model.ActiveDaysThisMonth = await monthTransactions
                    .Where(t => t.Date.HasValue)
                    .Select(t => t.Date!.Value.Date)
                    .Distinct()
                    .CountAsync();

                var monthApprovalsQuery = _context.DailyBranchApprovals
                    .AsNoTracking()
                    .Where(a =>
                        a.BranchId != null &&
                        a.BranchId.Trim() == normalizedBranchId &&
                        a.IsLocked &&
                        a.ApprovalDate.Date >= monthStart &&
                        a.ApprovalDate.Date <= today);

                model.ClosedDaysThisMonth = await monthApprovalsQuery.CountAsync();
                model.ClosureRateThisMonth = model.ActiveDaysThisMonth > 0
                    ? (decimal)model.ClosedDaysThisMonth * 100m / model.ActiveDaysThisMonth
                    : 0m;

                var todayApproval = await monthApprovalsQuery
                    .Where(a => a.ApprovalDate.Date == today)
                    .OrderByDescending(a => a.CreatedAt)
                    .FirstOrDefaultAsync();

                model.IsDayClosed = todayApproval?.IsLocked ?? false;
                model.ApprovedBy = todayApproval?.ApprovedBy;
                model.ClosedAtUtc = todayApproval?.LockedAt;

                var lastClosed = await _context.DailyBranchApprovals
                    .AsNoTracking()
                    .Where(a =>
                        a.BranchId != null &&
                        a.BranchId.Trim() == normalizedBranchId &&
                        a.IsLocked)
                    .OrderByDescending(a => a.ApprovalDate)
                    .ThenByDescending(a => a.CreatedAt)
                    .FirstOrDefaultAsync();

                if (lastClosed != null)
                {
                    model.LastClosedDate = lastClosed.ApprovalDate.Date;
                    model.LastClosedDebit = lastClosed.TotalDebit;
                    model.LastClosedCredit = lastClosed.TotalCredit;
                }
            }

            return View(model);
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
