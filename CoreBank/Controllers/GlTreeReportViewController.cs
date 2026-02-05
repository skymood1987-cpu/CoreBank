using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinCoreBank.Models.Dtos;
using MinCoreBank.Services;
using System.Collections.Generic;
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

        public async Task<IActionResult> Index(string branchId = null)
        {
            try
            {
                // Get user's branch code and role from claims
                var userBranchCode = User.FindFirst("BranchCode")?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var isAdmin = userRole?.ToLower() == "admin";

                // AUTO BRANCH SELECTION FOR ADMINS: Use bank-wide data
                IEnumerable<GlTreeDisplayDto> report;

                if (isAdmin)
                {
                    // ADMIN: Automatically show ALL branches data using bank-wide API
                    report = await _treeReportService.GetBankWideTreeReportAsync();
                    ViewBag.SelectedBranchId = "ALL"; // Indicate all branches
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
                    ViewBag.SelectedBranchId = userBranchCode;
                }

                // Pass data to view
                ViewBag.UserBranchCode = userBranchCode;
                ViewBag.UserRole = userRole;
                ViewBag.IsAdmin = isAdmin;

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