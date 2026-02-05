using Microsoft.EntityFrameworkCore;
using MinCoreBank.Data;
using MinCoreBank.Models;
using MinCoreBank.Models.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace MinCoreBank.Repositories
{
    public class GlTransactionRepository : IGlTransactionRepository
    {
        private readonly AppDbContext _context;
        private readonly IGeneralLedgerRepository _glRepository;

        public GlTransactionRepository(AppDbContext context, IGeneralLedgerRepository glRepository)
        {
            _context = context;
            _glRepository = glRepository;
        }

        public async Task<IEnumerable<GlTransaction>> GetAllAsync()
        {
            return await _context.GlTransactions
                .AsNoTracking()
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.UpdatedAt)
                .ToListAsync();
        }

        public async Task<GlTransaction> GetByIdAsync(long id)
        {
            return await _context.GlTransactions
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<GlTransaction> GettrasbybranchAsync(string branch_id)
        {
            return await _context.GlTransactions
                .Where(t => t.BranchId == branch_id)
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.UpdatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<GlTransaction> GettrasbybranchdateAsync(string branch_id, DateTime? date_)
        {
            return await _context.GlTransactions.FirstOrDefaultAsync(t => t.BranchId == branch_id && t.Date == date_);
        }

        public async Task<IEnumerable<GeneralLedgerAccount>> GetGlAccountsAsync()
        {
            return await _glRepository.GetAllAsync();
        }

        public async Task<GlTransaction> CreateAsync(GlTransaction transaction, string userId)
        {
            using var dbTransaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Set creation details
                transaction.CreatedBy = userId;
                transaction.UpdatedBy = userId;
                transaction.CreatedAt = DateTime.UtcNow;
                transaction.UpdatedAt = DateTime.UtcNow;

                // Add to context
                _context.GlTransactions.Add(transaction);
                await _context.SaveChangesAsync();

                // Now generate final binder number with the actual ID
                if (int.TryParse(transaction.BranchId, out int branchCode))
                {
                    string finalRef = BinderNumberGenerator.GenerateBinderNumber(branchCode, transaction.Id);
                    transaction.TransactionRef = finalRef;

                    // Update with final reference
                    _context.GlTransactions.Update(transaction);
                    await _context.SaveChangesAsync();
                }

                await dbTransaction.CommitAsync();
                return transaction;
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateAsync(GlTransaction transaction, string userId)
        {
            var baghdadTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Baghdad");
            var baghdadTime_now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, baghdadTimeZone);

            transaction.UpdatedAt = baghdadTime_now;
            transaction.UpdatedBy = userId;
            _context.GlTransactions.Update(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task ReverseAsync(long id, string userId)
        {
            var baghdadTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Baghdad");
            var baghdadTime_now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, baghdadTimeZone);

            var original = await GetByIdAsync(id);
            if (original == null) return;

            var reversal = new GlTransaction
            {
                GlId = original.GlId,
                GlName = $"Reversal of {original.GlName}",
                TransactionRef = $"RV{original.TransactionRef}",
                Date = baghdadTime_now,
                ValueDate = baghdadTime_now,
                DebitAccount = original.CreditAccount,
                CreditAccount = original.DebitAccount,
                Amount = original.Amount,
                AmountIqd = original.AmountIqd,
                Currency = original.Currency,
                FxRate = original.FxRate,
                CbiCode = original.CbiCode,
                DescriptionAr = $"تراجع عن {original.DescriptionAr}",
                DescriptionEn = $"Reversal of {original.DescriptionEn}",
                BranchId = original.BranchId,
                CreatedBy = userId,
                Status = "reversed",
                ReversalRef = original.Id,
                UpdatedBy = userId
            };

            original.Status = "reversed";
            await UpdateAsync(original, userId);
            await CreateAsync(reversal, userId);
        }

        public async Task<bool> ExistsAsync(long id)
        {
            return await _context.GlTransactions.AnyAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<GlTransaction>> GetByBranchAndDateRangeAsync(
            string branchId,
            DateTime startDate,
            DateTime endDate)
        {
            return await _context.GlTransactions
                .Where(t => t.BranchId == branchId &&
                           t.Date >= startDate &&
                           t.Date <= endDate)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}