// Services/IBranchLockService.cs
using System;
using System.Threading.Tasks;

namespace MinCoreBank.Services
{
    public interface IBranchLockService
    {
        Task<bool> IsTransactionLocked(long transactionId);
        Task<bool> CanModifyTransaction(long transactionId);
    }
}