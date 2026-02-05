// Services/BranchLockService.cs
using Microsoft.EntityFrameworkCore;
using MinCoreBank.Data;
using MinCoreBank.Repositories;
using System.Threading.Tasks;

namespace MinCoreBank.Services
{
    public class BranchLockService : IBranchLockService
    {
        private readonly AppDbContext _context;
        private readonly IDailyBranchApprovalRepository _approvalRepository;

        public BranchLockService(AppDbContext context, IDailyBranchApprovalRepository approvalRepository)
        {
            _context = context;
            _approvalRepository = approvalRepository;
        }

        public async Task<bool> IsTransactionLocked(long transactionId)
        {
            var transaction = await _context.GlTransactions
                .FirstOrDefaultAsync(t => t.Id == transactionId);

            if (transaction == null || transaction.Date == null)
                return false;

            return await _approvalRepository.IsDateLockedAsync(transaction.BranchId, transaction.Date.Value);
        }

        public async Task<bool> CanModifyTransaction(long transactionId)
        {
            return !await IsTransactionLocked(transactionId);
        }
    }
}