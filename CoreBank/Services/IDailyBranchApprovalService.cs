// Services/IDailyBranchApprovalService.cs
using MinCoreBank.Models.Dtos;
using System.Threading.Tasks;

namespace MinCoreBank.Services
{
    public interface IDailyBranchApprovalService
    {
        Task<DailyApprovalResponse> RecordDailyApprovalAsync(DailyBranchApprovalRequest request);
        Task<ApprovalStatusResponse> CheckApprovalStatusAsync(BranchApprovalStatusRequest request);
    }
}