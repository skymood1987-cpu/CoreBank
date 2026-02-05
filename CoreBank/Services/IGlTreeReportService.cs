// IGlTreeReportService.cs
using MinCoreBank.Models;
using MinCoreBank.Models.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;
using static MinCoreBank.Models.Dtos.GlTreeDisplayDto;

namespace MinCoreBank.Services
{
    public interface IGlTreeReportService
    {
        Task<IEnumerable<GlTreeReportDto>> GenerateTreeReportAsync(GlTreeReportRequest request);
        Task<IEnumerable<GlTreeDisplayDto>> GenerateFlatTreeReportAsync(GlTreeReportRequest request);
        Task<IEnumerable<GlTreeDisplayDto>> GetBranchTreeReportAsync(string branchId);
        Task<GlTreeReportDto> GetAccountHierarchyAsync(string glId, string branchId = null);
        Task<decimal> GetRolledUpBalanceAsync(string glId, string branchId = null);
        Task<IEnumerable<GlTreeDisplayDto>> GetBankWideTreeReportAsync();

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