using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinCoreBank.Models.Dtos;
using MinCoreBank.Services;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MinCoreBank.Controllers
{
    [Authorize]
    public class GlTreeReportViewController : Controller
    {
        private readonly IGlTreeReportService _treeReportService;

        public GlTreeReportViewController(IGlTreeReportService treeReportService)
        {
            _treeReportService = treeReportService;
        }

        public async Task<IActionResult> Index(string branchId = null, string accountNumber = null)
        {
            try
            {
                // Get user's branch code and role from claims
                var userBranchCode = User.FindFirst("BranchCode")?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var isAdmin = userRole?.ToLower() == "admin";
                var normalizedAccountNumber = accountNumber?.Trim();

                // AUTO BRANCH SELECTION FOR ADMINS: Use bank-wide data
                IEnumerable<GlTreeDisplayDto> report;
                var selectedBranchId = isAdmin ? null : userBranchCode;

                if (!string.IsNullOrWhiteSpace(normalizedAccountNumber))
                {
                    report = await _treeReportService.GenerateFlatTreeReportAsync(new GlTreeReportRequest
                    {
                        BranchId = selectedBranchId,
                        StartingGlId = normalizedAccountNumber
                    });
                }
                else
                {
                    if (isAdmin)
                    {
                        // ADMIN: Automatically show ALL branches data using bank-wide API
                        report = await _treeReportService.GetBankWideTreeReportAsync();
                    }
                    else
                    {
                        // NON-ADMIN: Show only their branch data
                        // If non-admin tries to access different branch, redirect to their branch
                        if (!string.IsNullOrEmpty(branchId) && branchId != userBranchCode)
                        {
                            return RedirectToAction("Index", new { branchId = userBranchCode });
                        }

                        report = await _treeReportService.GetBranchTreeReportAsync(userBranchCode);
                    }
                }

                // Pass data to view
                ViewBag.UserBranchCode = userBranchCode;
                ViewBag.UserRole = userRole;
                ViewBag.IsAdmin = isAdmin;
                ViewBag.SelectedBranchId = isAdmin ? "ALL" : userBranchCode;
                ViewBag.AccountNumberSearch = normalizedAccountNumber ?? string.Empty;
                ViewBag.SearchPerformed = !string.IsNullOrWhiteSpace(normalizedAccountNumber);
                ViewBag.NoSearchResults = !string.IsNullOrWhiteSpace(normalizedAccountNumber) && !report.Any();

                return View(report);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in Index: {ex.Message}");

                ViewBag.UserBranchCode = User.FindFirst("BranchCode")?.Value ?? "N/A";
                ViewBag.UserRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "user";
                ViewBag.IsAdmin = false;
                ViewBag.SelectedBranchId = null;
                return View(new List<GlTreeDisplayDto>());
            }
        }
    }
}
