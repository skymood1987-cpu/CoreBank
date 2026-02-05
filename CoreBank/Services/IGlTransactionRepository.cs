// Repositories/IGlTransactionRepository.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MinCoreBank.Models;

namespace MinCoreBank.Repositories
{
    public interface IGlTransactionRepository
    {
        /// <summary>
        /// Retrieves all GL transactions
        /// </summary>
        Task<IEnumerable<GlTransaction>> GetAllAsync();
        Task<GlTransaction> GettrasbybranchAsync(string branch_id);

        Task<GlTransaction> GettrasbybranchdateAsync(string branch_id, DateTime? date_);

        Task<IEnumerable<GlTransaction>> GetByBranchAndDateRangeAsync(string branchId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets a transaction by its ID
        /// </summary>
        /// <param name="id">Transaction ID</param>
        Task<GlTransaction> GetByIdAsync(long id);

        /// <summary>
        /// Creates a new transaction
        /// </summary>
        /// <param name="transaction">Transaction data</param>
        /// <param name="userId">User ID creating the transaction</param>
        Task<GlTransaction> CreateAsync(GlTransaction transaction, string userId);

        /// <summary>
        /// Updates an existing transaction
        /// </summary>
        /// <param name="transaction">Updated transaction data</param>
        /// <param name="userId">User ID making the update</param>
        Task UpdateAsync(GlTransaction transaction, string userId);

        /// <summary>
        /// Reverses a transaction by creating an opposite entry
        /// </summary>
        /// <param name="id">Original transaction ID to reverse</param>
        /// <param name="userId">User ID performing the reversal</param>
        Task ReverseAsync(long id, string userId);

        /// <summary>
        /// Checks if a transaction exists
        /// </summary>
        /// <param name="id">Transaction ID to check</param>
        Task<bool> ExistsAsync(long id);

        /// <summary>
        /// Gets all GL accounts for dropdown
        /// </summary>
        Task<IEnumerable<GeneralLedgerAccount>> GetGlAccountsAsync();
    }
}