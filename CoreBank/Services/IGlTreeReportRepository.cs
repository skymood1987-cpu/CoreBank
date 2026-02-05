// IGlTreeReportRepository.cs
using MinCoreBank.Models;
using MinCoreBank.Models.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;
using static MinCoreBank.Models.Dtos.GlTreeDisplayDto;

namespace MinCoreBank.Repositories
{
    public interface IGlTreeReportRepository
    {
        Task<IEnumerable<GlTreeReportDto>> GetGlTreeReportAsync(GlTreeReportRequest request);
        Task<IEnumerable<GlTreeReportDto>> GetGlTreeByBranchAsync(string branchId);
        Task<IEnumerable<GlTreeReportDto>> GetGlTreeByParentAsync(string parentGlId);
        Task<GlTreeReportDto> GetGlNodeDetailsAsync(string glId, string branchId = null);
        Task<IEnumerable<GlTreeDisplayDto>> GetFlatTreeReportAsync(GlTreeReportRequest request);
        Task<IEnumerable<GlTreeDisplayDto>> GetBankWideFlatTreeReportAsync();

        // Existing transaction methods
        Task<IEnumerable<TransactionDetailDto>> GetTransactionsByRefAndBranchAsync(TransactionQueryRequest request);
        Task<IEnumerable<TransactionDetailDto>> GetTransactionsByDateRangeAndBranchAsync(TransactionQueryRequest request);
        Task<IEnumerable<TransactionDetailDto>> GetTransactionsByRefDateRangeAndBranchAsync(TransactionQueryRequest request);

        // NEW GL Account transaction methods
        Task<IEnumerable<TransactionDetailDto>> GetTransactionsByGlIdAndBranchAsync(TransactionQueryRequest request);
        Task<IEnumerable<TransactionDetailDto>> GetTransactionsByGlIdDateRangeAndBranchAsync(TransactionQueryRequest request);

        Task<List<Branch>> GetAvailableBranchesAsync();
    }
}