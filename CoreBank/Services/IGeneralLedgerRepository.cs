using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MinCoreBank.Models;

namespace MinCoreBank.Repositories
{
    public interface IGeneralLedgerRepository
    {
        Task<IEnumerable<GeneralLedgerAccount>> GetAllAsync();
        Task<GeneralLedgerAccount> GetByIdAsync(int id);
        Task<GeneralLedgerAccount> CreateAsync(GeneralLedgerAccount account, string userId);
        Task UpdateAsync(GeneralLedgerAccount account, string userId);
        Task SoftDeleteAsync(int id, string username);
        Task<bool> ExistsAsync(int id);
        Task<bool> AccountIdExistsAsync(int id);
    }
}