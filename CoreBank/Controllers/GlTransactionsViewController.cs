// Controllers/GlTransactionsViewController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinCoreBank.Models;
using MinCoreBank.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MinCoreBank.Controllers
{
    [Authorize]
    public class GlTransactionsViewController : Controller
    {
        private readonly IGlTransactionRepository _repository;
        private readonly IGeneralLedgerRepository _glRepository;

        private const int PageSize = 10; // 10 rows per page

        public GlTransactionsViewController(IGlTransactionRepository repository, IGeneralLedgerRepository glRepository)
        {
            _repository = repository;
            _glRepository = glRepository;
        }

        public async Task<IActionResult> Index(int page = 1, string branchId = null, string startDate = null, string endDate = null)
        {
            // Get the user's branch code from claims
            var userBranchCode = User.FindFirst("BranchCode")?.Value;

            // If no branchId filter is provided, default to the user's branch
            if (string.IsNullOrEmpty(branchId) && !string.IsNullOrEmpty(userBranchCode))
            {
                branchId = userBranchCode;
            }

            // Get all transactions
            var allTransactions = await _repository.GetAllAsync();
            var transactionsList = allTransactions.ToList();

            // Apply filters if provided
            if (!string.IsNullOrEmpty(branchId) && branchId != "all")
            {
                transactionsList = transactionsList.Where(t => t.BranchId == branchId).ToList();
            }

            var today = System.DateTime.Today;
            transactionsList = transactionsList
                .Where(t => t.Date.HasValue && t.Date.Value.Date == today)
                .ToList();

            // Daily summary values are based on the same "today/business-day" dataset shown in the table.
            var totalDebitToday = transactionsList.Sum(t => t.DebitAccount ?? 0m);
            var totalCreditToday = transactionsList.Sum(t => t.CreditAccount ?? 0m);
            var isBalancedToday = totalDebitToday == totalCreditToday;
            var dailyDifference = totalCreditToday - totalDebitToday;

            // Get GL accounts for dropdown
            var glAccounts = await _glRepository.GetAllAsync();
            ViewBag.GlAccounts = glAccounts;

            // Calculate pagination
            var totalCount = transactionsList.Count;
            var totalPages = System.Math.Max(1, (int)System.Math.Ceiling(totalCount / (double)PageSize));

            // Ensure page is within valid range
            page = page < 1 ? 1 : page > totalPages ? totalPages : page;

            // Get transactions for current page
            var pagedTransactions = transactionsList
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            // Pass data to view
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageSize = PageSize;
            ViewBag.BranchId = branchId;
            ViewBag.StartDate = today.ToString("yyyy-MM-dd");
            ViewBag.EndDate = today.ToString("yyyy-MM-dd");
            ViewBag.UserBranchCode = userBranchCode; // Pass user's branch code to the view
            ViewBag.TotalDebitToday = totalDebitToday;
            ViewBag.TotalCreditToday = totalCreditToday;
            ViewBag.IsBalancedToday = isBalancedToday;
            ViewBag.DailyDifference = dailyDifference;

            return View(pagedTransactions);
        }

        public async Task<IActionResult> Create()
        {
            // Fetch GL accounts for dropdown
            var glAccounts = await _glRepository.GetAllAsync();
            ViewBag.GlAccounts = glAccounts;
            return View();
        }

        public async Task<IActionResult> Details(long id)
        {
            var transaction = await _repository.GetByIdAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }
            return View(transaction);
        }

        public async Task<IActionResult> Edit(long id)
        {
            var transaction = await _repository.GetByIdAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }

            // Fetch GL accounts for dropdown in edit mode too
            var glAccounts = await _glRepository.GetAllAsync();
            ViewBag.GlAccounts = glAccounts;

            return View(transaction);
        }

        public async Task<IActionResult> Reverse(long id)
        {
            var transaction = await _repository.GetByIdAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }
            return View(transaction);
        }
    }
}
