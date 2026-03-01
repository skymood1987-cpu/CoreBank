using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinCoreBank.Models;
using MinCoreBank.Repositories;

namespace MinCoreBank.Controllers
{
    [Authorize]
    public class GlTransactionsViewController : Controller
    {
        private readonly IGlTransactionRepository _repository;
        private readonly IGeneralLedgerRepository _glRepository;
        private const int PageSize = 10;

        public GlTransactionsViewController(IGlTransactionRepository repository, IGeneralLedgerRepository glRepository)
        {
            _repository = repository;
            _glRepository = glRepository;
        }

        public async Task<IActionResult> Index(int page = 1, string branchId = null, string period = "today", string from = null, string to = null)
        {
            var userBranchCode = User.FindFirst("BranchCode")?.Value;
            if (string.IsNullOrEmpty(branchId) && !string.IsNullOrEmpty(userBranchCode))
            {
                branchId = userBranchCode;
            }

            var transactionsList = (await _repository.GetAllAsync()).ToList();

            if (!string.IsNullOrEmpty(branchId) && branchId != "all")
            {
                transactionsList = transactionsList.Where(t => t.BranchId == branchId).ToList();
            }

            var today = DateTime.Today;
            DateTime startDate;
            DateTime endDate;
            switch ((period ?? "today").ToLower())
            {
                case "week":
                    var diff = (7 + (today.DayOfWeek - DayOfWeek.Saturday)) % 7;
                    startDate = today.AddDays(-diff);
                    endDate = startDate.AddDays(6);
                    break;
                case "month":
                    startDate = new DateTime(today.Year, today.Month, 1);
                    endDate = startDate.AddMonths(1).AddDays(-1);
                    break;
                case "custom":
                    if (!DateTime.TryParse(from, out startDate) || !DateTime.TryParse(to, out endDate))
                    {
                        startDate = today;
                        endDate = today;
                        period = "today";
                    }
                    break;
                default:
                    period = "today";
                    startDate = today;
                    endDate = today;
                    break;
            }

            transactionsList = transactionsList
                .Where(t => t.Date.HasValue && t.Date.Value.Date >= startDate.Date && t.Date.Value.Date <= endDate.Date)
                .OrderByDescending(t => t.CreatedAt)
                .ToList();

            var totalCredit = transactionsList.Sum(t => t.CreditAccount ?? 0m);
            var totalDebit = transactionsList.Sum(t => t.DebitAccount ?? 0m);
            var balanced = Math.Abs(totalCredit - totalDebit) <= 0.001m;

            var totalCount = transactionsList.Count;
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));
            page = Math.Max(1, Math.Min(page, totalPages));

            var pagedTransactions = transactionsList.Skip((page - 1) * PageSize).Take(PageSize).ToList();

            ViewBag.GlAccounts = await _glRepository.GetAllAsync();
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageSize = PageSize;
            ViewBag.BranchId = branchId;
            ViewBag.UserBranchCode = userBranchCode;

            var vm = new GlTransactionsIndexViewModel
            {
                Transactions = pagedTransactions,
                Period = period ?? "today",
                From = startDate,
                To = endDate,
                TotalCredit = totalCredit,
                TotalDebit = totalDebit,
                BalanceStatus = balanced ? "متوازنة" : "غير متوازنة"
            };

            return View(vm);
        }

        [HttpGet("api/gltransactionsview/account/{id:int}")]
        public async Task<IActionResult> LookupAccount(int id)
        {
            var account = await _glRepository.GetByIdAsync(id);
            if (account == null)
            {
                return NotFound(new { message = "رقم الأستاذ العام غير موجود" });
            }

            return Ok(new { id = account.Id, nameAr = account.NameAr });
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.GlAccounts = await _glRepository.GetAllAsync();
            return View();
        }

        public async Task<IActionResult> Details(long id)
        {
            var transaction = await _repository.GetByIdAsync(id);
            if (transaction == null) return NotFound();
            return View(transaction);
        }

        public async Task<IActionResult> Edit(long id)
        {
            var transaction = await _repository.GetByIdAsync(id);
            if (transaction == null) return NotFound();
            ViewBag.GlAccounts = await _glRepository.GetAllAsync();
            return View(transaction);
        }

        public async Task<IActionResult> Reverse(long id)
        {
            var transaction = await _repository.GetByIdAsync(id);
            if (transaction == null) return NotFound();
            return View(transaction);
        }
    }
}
