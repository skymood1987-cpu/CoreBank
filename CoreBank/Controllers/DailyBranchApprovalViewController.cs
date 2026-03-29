using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinCoreBank.Models;
using MinCoreBank.Models.ViewModels;
using MinCoreBank.Repositories;
using MinCoreBank.Data;
using MinCoreBank.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MinCoreBank.Controllers
{
   [Authorize]
    public class DailyBranchApprovalViewController : Controller
    {
        private readonly IDailyBranchApprovalRepository _approvalRepository;
        private readonly AppDbContext _context;
        private readonly IUserService _userService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ILogger<DailyBranchApprovalViewController> _logger;

        public DailyBranchApprovalViewController(
            IDailyBranchApprovalRepository approvalRepository,
            AppDbContext context,
            IUserService userService,
            IPasswordHasher passwordHasher,
            ILogger<DailyBranchApprovalViewController> logger)
        {
            _approvalRepository = approvalRepository;
            _context = context;
            _userService = userService;
            _passwordHasher = passwordHasher;
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveToday([FromBody] DailyApprovalRequest request)
        {
            try
            {
                if (request == null)
                {
                    return Json(new { success = false, message = "بيانات الاعتماد غير صالحة" });
                }

                // Get branch from user claims
                var userBranchCode = User.FindFirst("BranchCode")?.Value;
                if (string.IsNullOrEmpty(userBranchCode))
                {
                    return Json(new { success = false, message = "لم يتم العثور على رمز الفرع للمستخدم" });
                }

                var managerUsername = request.ManagerUsername?.Trim();
                var managerPassword = request.ManagerPassword;

                if (string.IsNullOrWhiteSpace(managerUsername) || string.IsNullOrWhiteSpace(managerPassword))
                {
                    return Json(new { success = false, message = "يرجى إدخال اسم المستخدم وكلمة المرور للمدير" });
                }

                var managerLookup = await _userService.GetUserByUsername(managerUsername);
                Users? manager = managerLookup.Success ? managerLookup.Data : null;

                if (manager == null && managerUsername.StartsWith("USER-", StringComparison.OrdinalIgnoreCase))
                {
                    var managerById = await _userService.GetUserById(managerUsername);
                    manager = managerById.Success ? managerById.Data : null;
                }

                if (manager == null)
                {
                    _logger.LogWarning("Daily approval rejected. Manager not found. Username: {ManagerUsername}, Branch: {Branch}", managerUsername, userBranchCode);
                    return Json(new { success = false, message = "اسم المستخدم للمدير غير صحيح" });
                }

                if (!string.Equals(manager.Status?.Trim(), "active", StringComparison.OrdinalIgnoreCase))
                {
                    return Json(new { success = false, message = "حساب المدير غير نشط" });
                }

                if (!string.Equals(manager.BranchId?.Trim(), userBranchCode?.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Daily approval rejected. Manager branch mismatch. Username: {ManagerUsername}, ManagerBranch: {ManagerBranch}, RequestedBranch: {RequestedBranch}",
                        managerUsername, manager.BranchId, userBranchCode);
                    return Json(new { success = false, message = "المدير لا ينتمي إلى نفس الفرع" });
                }

                var role = manager.Role?.Trim().ToLowerInvariant();
                if (role != "manager" && role != "مدير")
                {
                    return Json(new { success = false, message = "المستخدم المدخل ليس بصلاحية مدير" });
                }

                if (!_passwordHasher.VerifyPassword(manager.password_hash, managerPassword))
                {
                    _logger.LogWarning("Daily approval rejected. Invalid manager password. Username: {ManagerUsername}, ManagerId: {ManagerId}", managerUsername, manager.Id);
                    return Json(new { success = false, message = "اسم المستخدم أو كلمة المرور غير صحيحة" });
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
                    ApprovedBy = string.IsNullOrWhiteSpace(manager.Name_ar) ? manager.Name_en : manager.Name_ar,
                    TotalCredit = balanceResult.TotalCredit,
                    TotalDebit = balanceResult.TotalDebit,
                    IsLocked = true, // Auto-lock when approved
                    LockedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                await _approvalRepository.RecordDailyApprovalAsync(approval);

                _logger.LogInformation(
                    "Daily branch approval completed. Branch: {BranchId}, Manager: {ManagerId}/{ManagerName}, RequestedBy: {RequestedBy}, At: {TimestampUtc}",
                    userBranchCode,
                    manager.Id,
                    approval.ApprovedBy,
                    User.Identity?.Name ?? "Unknown",
                    DateTime.UtcNow);

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
