using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinCoreBank.Models;
using MinCoreBank.Models.ViewModels;
using MinCoreBank.Repositories;
using MinCoreBank.Data;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MinCoreBank.Controllers
{
   [Authorize]
    public class DailyBranchApprovalViewController : Controller
    {
        private readonly IDailyBranchApprovalRepository _approvalRepository;
        private readonly AppDbContext _context;
        private readonly ILogger<DailyBranchApprovalViewController> _logger;

        public DailyBranchApprovalViewController(
            IDailyBranchApprovalRepository approvalRepository,
            AppDbContext context,
            ILogger<DailyBranchApprovalViewController> logger)
        {
            _approvalRepository = approvalRepository;
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Route("DailyBranchApprovals")]
        public async Task<IActionResult> Index()
        {
            try
            {
                // Get branch from user claims
                var userBranchCode = User.FindFirst("BranchCode")?.Value;
                if (string.IsNullOrEmpty(userBranchCode))
                {
                    TempData["Error"] = "لم يتم العثور على رمز الفرع للمستخدم";
                    return View(new DailyBranchApprovalViewModel());
                }

                var today = DateTime.Today;

                // Check if today is already approved
                var isApprovedToday = await _approvalRepository.HasBranchBeenApprovedTodayAsync(userBranchCode);
                var todayApproval = await _approvalRepository.GetTodayApprovalAsync(userBranchCode);

                // Get today's balance
                var balanceResult = await _approvalRepository.GetBranchDailyBalanceAsync(userBranchCode, today);

                var viewModel = new DailyBranchApprovalViewModel
                {
                    BranchId = userBranchCode,
                    Today = today,
                    IsApprovedToday = isApprovedToday,
                    TodayApproval = todayApproval,
                    TotalCredit = balanceResult.TotalCredit,
                    TotalDebit = balanceResult.TotalDebit,
                    IsBalanced = balanceResult.IsBalanced
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading daily branch approvals");
                TempData["Error"] = "حدث خطأ أثناء تحميل الموافقات";
                return View(new DailyBranchApprovalViewModel());
            }
        }

        [HttpPost]
        [Route("DailyBranchApprovals/ApproveToday")]
        public async Task<IActionResult> ApproveToday([FromBody] DailyApprovalRequest request)
        {
            try
            {
                // Get branch from user claims
                var userBranchCode = User.FindFirst("BranchCode")?.Value;
                if (string.IsNullOrEmpty(userBranchCode))
                {
                    return Json(new { success = false, message = "لم يتم العثور على رمز الفرع للمستخدم" });
                }

                var today = DateTime.Today;

                // Check if already approved today
                if (await _approvalRepository.HasBranchBeenApprovedTodayAsync(userBranchCode))
                {
                    return Json(new { success = false, message = "تمت الموافقة على الفرع اليوم بالفعل" });
                }

                // Get today's balance
                var balanceResult = await _approvalRepository.GetBranchDailyBalanceAsync(userBranchCode, today);

                // Create approval record
                var approval = new DailyBranchApproval
                {
                    BranchId = userBranchCode,
                    ApprovalDate = today,
                    ApprovedBy = User.Identity.Name ?? "System",
                    TotalCredit = balanceResult.TotalCredit,
                    TotalDebit = balanceResult.TotalDebit,
                    IsLocked = true, // Auto-lock when approved
                    LockedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                await _approvalRepository.RecordDailyApprovalAsync(approval);

                return Json(new
                {
                    success = true,
                    message = "تمت الموافقة على الفرع اليوم بنجاح",
                    totalCredit = balanceResult.TotalCredit,
                    totalDebit = balanceResult.TotalDebit,
                    isBalanced = balanceResult.IsBalanced
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving branch for today");
                return Json(new { success = false, message = "حدث خطأ أثناء الموافقة على الفرع" });
            }
        }
    }
}