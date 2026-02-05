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

            if (!string.IsNullOrEmpty(startDate) && System.DateTime.TryParse(startDate, out var startDateParsed))
            {
                transactionsList = transactionsList.Where(t => t.Date >= startDateParsed).ToList();
            }

            if (!string.IsNullOrEmpty(endDate) && System.DateTime.TryParse(endDate, out var endDateParsed))
            {
                transactionsList = transactionsList.Where(t => t.Date <= endDateParsed).ToList();
            }

            // Get GL accounts for dropdown
            var glAccounts = await _glRepository.GetAllAsync();
            ViewBag.GlAccounts = glAccounts;

            // Calculate pagination
            var totalCount = transactionsList.Count;
            var totalPages = (int)System.Math.Ceiling(totalCount / (double)PageSize);

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
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.UserBranchCode = userBranchCode; // Pass user's branch code to the view

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