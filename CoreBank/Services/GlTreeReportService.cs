// GlTreeReportService.cs
using MinCoreBank.Models;
using MinCoreBank.Models.Dtos;
using MinCoreBank.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using static MinCoreBank.Models.Dtos.GlTreeDisplayDto;

namespace MinCoreBank.Services
{
    public class GlTreeReportService : IGlTreeReportService
    {
        private readonly IGlTreeReportRepository _repository;

        public GlTreeReportService(IGlTreeReportRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Branch>> GetAvailableBranchesAsync()
        {
            return await _repository.GetAvailableBranchesAsync();
        }

        public async Task<IEnumerable<GlTreeReportDto>> GenerateTreeReportAsync(GlTreeReportRequest request)
        {
            return await _repository.GetGlTreeReportAsync(request);
        }

        public async Task<IEnumerable<GlTreeDisplayDto>> GenerateFlatTreeReportAsync(GlTreeReportRequest request)
        {
            return await _repository.GetFlatTreeReportAsync(request);
        }

        public async Task<IEnumerable<GlTreeDisplayDto>> GenerateCreditDebtTreeReportAsync(GlTreeReportRequest request)
        {
            return await _repository.GetFlatTreeReportAsync(request);
        }

        public async Task<GlTreeReportDto> GetAccountHierarchyAsync(string glId, string branchId = null)
        {
            return await _repository.GetGlNodeDetailsAsync(glId, branchId);
        }

        public async Task<decimal> GetRolledUpBalanceAsync(string glId, string branchId = null)
        {
            var account = await _repository.GetGlNodeDetailsAsync(glId, branchId);
            return account?.Balance ?? 0;
        }

        public async Task<IEnumerable<GlTreeDisplayDto>> GetBranchTreeReportAsync(string branchId)
        {
            var request = new GlTreeReportRequest { BranchId = branchId };
            return await _repository.GetFlatTreeReportAsync(request);
        }

        public async Task<IEnumerable<GlTreeDisplayDto>> GetBankWideTreeReportAsync()
        {
            return await _repository.GetBankWideFlatTreeReportAsync();
        }

        // Existing transaction query methods
        public async Task<IEnumerable<TransactionDetailDto>> GetTransactionsByRefAndBranchAsync(TransactionQueryRequest request)
        {
            return await _repository.GetTransactionsByRefAndBranchAsync(request);
        }

        public async Task<IEnumerable<TransactionDetailDto>> GetTransactionsByDateRangeAndBranchAsync(TransactionQueryRequest request)
        {
            return await _repository.GetTransactionsByDateRangeAndBranchAsync(request);
        }

        public async Task<IEnumerable<TransactionDetailDto>> GetTransactionsByRefDateRangeAndBranchAsync(TransactionQueryRequest request)
        {
            return await _repository.GetTransactionsByRefDateRangeAndBranchAsync(request);
        }

        // NEW GL Account transaction query methods
        public async Task<IEnumerable<TransactionDetailDto>> GetTransactionsByGlIdAndBranchAsync(TransactionQueryRequest request)
        {
            return await _repository.GetTransactionsByGlIdAndBranchAsync(request);
        }

        public async Task<IEnumerable<TransactionDetailDto>> GetTransactionsByGlIdDateRangeAndBranchAsync(TransactionQueryRequest request)
        {
            return await _repository.GetTransactionsByGlIdDateRangeAndBranchAsync(request);
        }
    }
}