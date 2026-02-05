// Services/DailyBranchApprovalService.cs
using MinCoreBank.Models;
using MinCoreBank.Models.Dtos;
using MinCoreBank.Repositories;
using System.Threading.Tasks;

namespace MinCoreBank.Services
{
    public class DailyBranchApprovalService : IDailyBranchApprovalService
    {
        private readonly IDailyBranchApprovalRepository _approvalRepository;
        private readonly ILogger<DailyBranchApprovalService> _logger;

        public DailyBranchApprovalService(IDailyBranchApprovalRepository approvalRepository, ILogger<DailyBranchApprovalService> logger)
        {
            _approvalRepository = approvalRepository;
            _logger = logger;
        }

        public async Task<DailyApprovalResponse> RecordDailyApprovalAsync(DailyBranchApprovalRequest request)
        {
            try
            {
                // Check if already approved today
                if (await _approvalRepository.HasBranchBeenApprovedTodayAsync(request.BranchId))
                {
                    return new DailyApprovalResponse
                    {
                        Success = false,
                        Message = "Daily approval already recorded for today",
                        IsLocked = true
                    };
                }

                // Validate credit = debit
                var balanceResult = await _approvalRepository.GetBranchDailyBalanceAsync(request.BranchId);

                if (!balanceResult.IsBalanced)
                {
                    return new DailyApprovalResponse
                    {
                        Success = false,
                        Message = $"Cannot approve - Branch not balanced. Credit: {balanceResult.TotalCredit}, Debit: {balanceResult.TotalDebit}",
                        TotalCredit = balanceResult.TotalCredit,
                        TotalDebit = balanceResult.TotalDebit,
                        IsBalanced = false,
                        IsLocked = false
                    };
                }

                // Record approval with HARD LOCK
                var approval = new DailyBranchApproval
                {
                    BranchId = request.BranchId,
                    ApprovalDate = DateTime.UtcNow.Date,
                    ApprovedBy = request.ApprovedBy,
                    IsLocked = true,
                    LockedAt = DateTime.UtcNow,
                    TotalCredit = balanceResult.TotalCredit,
                    TotalDebit = balanceResult.TotalDebit,
                    CreatedAt = DateTime.UtcNow
                };

                await _approvalRepository.RecordDailyApprovalAsync(approval);

                return new DailyApprovalResponse
                {
                    Success = true,
                    Message = "Branch daily approval recorded successfully - DAY LOCKED",
                    TotalCredit = balanceResult.TotalCredit,
                    TotalDebit = balanceResult.TotalDebit,
                    IsBalanced = true,
                    IsLocked = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error recording daily approval: {ex.Message}");
                return new DailyApprovalResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<ApprovalStatusResponse> CheckApprovalStatusAsync(BranchApprovalStatusRequest request)
        {
            try
            {
                var isApprovedToday = await _approvalRepository.HasBranchBeenApprovedTodayAsync(request.BranchId);
                var balanceResult = await _approvalRepository.GetBranchDailyBalanceAsync(request.BranchId);

                return new ApprovalStatusResponse
                {
                    IsApprovedToday = isApprovedToday,
                    IsLocked = isApprovedToday,
                    TotalCredit = balanceResult.TotalCredit,
                    TotalDebit = balanceResult.TotalDebit,
                    IsBalanced = balanceResult.IsBalanced
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking approval status: {ex.Message}");
                return new ApprovalStatusResponse
                {
                    IsApprovedToday = false,
                    IsLocked = false,
                    IsBalanced = false
                };
            }
        }
    }
}