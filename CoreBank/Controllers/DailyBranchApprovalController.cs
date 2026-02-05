using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinCoreBank.Models.Dtos;
using MinCoreBank.Services;
using MinCoreBank.Repositories;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MinCoreBank.Controllers
{
   [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DailyBranchApprovalController : ControllerBase
    {
        private readonly IDailyBranchApprovalService _approvalService;
        private readonly IBranchLockService _lockService;
        private readonly IDailyBranchApprovalRepository _approvalRepository;
        private readonly ILogger<DailyBranchApprovalController> _logger;

        public DailyBranchApprovalController(
            IDailyBranchApprovalService approvalService,
            IBranchLockService lockService,
            IDailyBranchApprovalRepository approvalRepository,
            ILogger<DailyBranchApprovalController> logger)
        {
            _approvalService = approvalService;
            _lockService = lockService;
            _approvalRepository = approvalRepository;
            _logger = logger;
        }
        [HttpPost("RecordDailyApproval")]
        public async Task<IActionResult> RecordDailyApproval([FromBody] DailyBranchApprovalRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Success = false, Message = "Invalid request data" });
            }

            try
            {
                var userName = User.Identity.Name;
                if (string.IsNullOrEmpty(userName))
                {
                    return Unauthorized(new { Success = false, Message = "User not authenticated" });
                }

                request.ApprovedBy = userName;

                var result = await _approvalService.RecordDailyApprovalAsync(request);

                if (result.Success)
                {
                    return Ok(new
                    {
                        Success = true,
                        result.Message,
                        result.TotalCredit,
                        result.TotalDebit,
                        result.IsBalanced,
                        result.IsLocked
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        Success = false,
                        result.Message,
                        result.TotalCredit,
                        result.TotalDebit,
                        result.IsBalanced,
                        result.IsLocked
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in RecordDailyApproval: {ex.Message}");
                return StatusCode(500, new { Success = false, Message = "An error occurred while processing your request" });
            }
        }

        [HttpPost("CheckApprovalStatus")]
        public async Task<IActionResult> CheckApprovalStatus([FromBody] BranchApprovalStatusRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { IsApprovedToday = false, IsLocked = false });
            }

            try
            {
                var result = await _approvalService.CheckApprovalStatusAsync(request);

                return Ok(new
                {
                    result.IsApprovedToday,
                    result.IsLocked,
                    result.TotalCredit,
                    result.TotalDebit,
                    result.IsBalanced
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in CheckApprovalStatus: {ex.Message}");
                return StatusCode(500, new { IsApprovedToday = false, IsLocked = false });
            }
        }

       // Controllers/DailyBranchApprovalController.cs


            [HttpGet("GetBranchDailyBalance/{branchId}/{date?}")]
            public async Task<IActionResult> GetBranchDailyBalance(string branchId, string date = null)
            {
                try
                {
                    DateTime targetDate = DateTime.Today;

                    if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime parsedDate))
                    {
                        targetDate = parsedDate;
                    }

                    // Use the repository method that accepts a specific date
                    var balanceResult = await _approvalRepository.GetBranchDailyBalanceAsync(branchId, targetDate);

                    var isApproved = await _approvalRepository.IsDateLockedAsync(branchId, targetDate);

                    return Ok(new
                    {
                        balanceResult.TotalCredit,
                        balanceResult.TotalDebit,
                        balanceResult.IsBalanced,
                        Difference = balanceResult.TotalCredit - balanceResult.TotalDebit,
                        IsApprovedToday = isApproved,
                        IsLocked = isApproved,
                        CheckedDate = targetDate.ToString("yyyy-MM-dd")
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetBranchDailyBalance: {ex.Message}");
                    return StatusCode(500, new { Message = "Error retrieving branch daily balance" });
                }
            }

            [HttpGet("CanModifyTransaction/{transactionId}")]
        public async Task<IActionResult> CanModifyTransaction(long transactionId)
        {
            try
            {
                var canModify = await _lockService.CanModifyTransaction(transactionId);
                return Ok(new { CanModify = canModify });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in CanModifyTransaction: {ex.Message}");
                return StatusCode(500, new { CanModify = false });
            }
        }
    }
}