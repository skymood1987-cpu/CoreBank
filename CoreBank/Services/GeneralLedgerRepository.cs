using Microsoft.EntityFrameworkCore;
using MinCoreBank.Data;
using MinCoreBank.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinCoreBank.Repositories
{
    public class GeneralLedgerRepository : IGeneralLedgerRepository
    {
        private readonly AppDbContext _context;

        public GeneralLedgerRepository(AppDbContext context)
        {
            _context = context;
        }


        public async Task<bool> AccountIdExistsAsync(int id)
        {
            return await _context.GeneralLedgerAccounts.AnyAsync(a => a.Id == id);
        }
        public async Task<IEnumerable<GeneralLedgerAccount>> GetAllAsync()
        {
            try
            {
                return await _context.GeneralLedgerAccounts
                    
                    .AsNoTracking()  // Improves performance for read-only operations
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                // Log the exception details including the SQL query
                var sql = _context.GeneralLedgerAccounts.ToQueryString();
                Console.WriteLine($"SQL Query: {sql}");
                Console.WriteLine($"Error: {ex}");
                throw;  // Re-throw after logging
            }
        }
        

        public async Task<GeneralLedgerAccount> GetByIdAsync(int id)
        {
            return await _context.GeneralLedgerAccounts
                
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<GeneralLedgerAccount> CreateAsync(GeneralLedgerAccount account, string userId)
        {
            account.CreatedAt = DateTime.UtcNow;
            account.UpdatedAt = DateTime.UtcNow;
            account.UpdatedBy = userId;

            _context.GeneralLedgerAccounts.Add(account);
            await _context.SaveChangesAsync();
            return account;
        }

        public async Task UpdateAsync(GeneralLedgerAccount account, string userId)
        {
            account.UpdatedAt = DateTime.UtcNow;
            account.UpdatedBy = userId;

            _context.GeneralLedgerAccounts.Update(account);
            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(int id, string username)
        {
            var account = await GetByIdAsync(id);
            if (account != null)
            {
                // Update status to "Disabled" instead of removing
                account.Status = "closed";
                account.UpdatedAt = DateTime.UtcNow;
                account.UpdatedBy = username;

                // Mark as modified and save
                _context.Entry(account).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.GeneralLedgerAccounts.AnyAsync(a => a.Id == id);
        }
    }
}