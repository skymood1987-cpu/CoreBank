using Microsoft.EntityFrameworkCore;
using MinCoreBank.Data;
using MinCoreBank.Models;
using MinCoreBank.Models.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static MinCoreBank.Models.Dtos.GlTreeDisplayDto;

namespace MinCoreBank.Repositories
{
    public class GlTreeReportRepository : IGlTreeReportRepository
    {
        private readonly AppDbContext _context;

        public GlTreeReportRepository(AppDbContext context)
        {
            _context = context;
        }
        // Add these methods to GlTreeReportRepository.cs class:

        public async Task<IEnumerable<TransactionDetailDto>> GetTransactionsByGlIdAndBranchAsync(TransactionQueryRequest request)
        {
            var query = _context.GlTransactions
                .Where(t => t.Status == "completed");

            // Apply branch filter
            if (!string.IsNullOrEmpty(request.BranchId))
            {
                query = query.Where(t => t.BranchId == request.BranchId);
            }

            // Apply GL account filter
            if (!string.IsNullOrEmpty(request.GlId))
            {
                query = query.Where(t => t.GlId == request.GlId);
            }

            var transactions = await query
                .OrderByDescending(t => t.Date)
                .Select(t => new TransactionDetailDto
                {
                    Id = t.Id,
                    GlId = t.GlId,
                    GlName = t.GlName,
                    TransactionRef = t.TransactionRef,
                    Date = t.Date,
                    ValueDate = t.ValueDate,
                    DebitAccount = t.DebitAccount,
                    CreditAccount = t.CreditAccount,
                    Amount = t.Amount,
                    AmountIqd = t.AmountIqd,
                    Currency = t.Currency,
                    FxRate = t.FxRate,
                    CbiCode = t.CbiCode,
                    DescriptionAr = t.DescriptionAr,
                    DescriptionEn = t.DescriptionEn,
                    BranchId = t.BranchId,
                    CreatedBy = t.CreatedBy,
                    Status = t.Status,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            return transactions;
        }

        public async Task<IEnumerable<TransactionDetailDto>> GetTransactionsByGlIdDateRangeAndBranchAsync(TransactionQueryRequest request)
        {
            var query = _context.GlTransactions
                .Where(t => t.Status == "completed");

            // Apply branch filter
            if (!string.IsNullOrEmpty(request.BranchId))
            {
                query = query.Where(t => t.BranchId == request.BranchId);
            }

            // Apply GL account filter
            if (!string.IsNullOrEmpty(request.GlId))
            {
                query = query.Where(t => t.GlId == request.GlId);
            }

            // Apply date range filter
            if (request.StartDate.HasValue)
            {
                query = query.Where(t => t.Date >= request.StartDate.Value);
            }

            if (request.EndDate.HasValue)
            {
                var endDate = request.EndDate.Value.Date.AddDays(1).AddSeconds(-1);
                query = query.Where(t => t.Date <= endDate);
            }

            var transactions = await query
                .OrderByDescending(t => t.Date)
                .Select(t => new TransactionDetailDto
                {
                    Id = t.Id,
                    GlId = t.GlId,
                    GlName = t.GlName,
                    TransactionRef = t.TransactionRef,
                    Date = t.Date,
                    ValueDate = t.ValueDate,
                    DebitAccount = t.DebitAccount,
                    CreditAccount = t.CreditAccount,
                    Amount = t.Amount,
                    AmountIqd = t.AmountIqd,
                    Currency = t.Currency,
                    FxRate = t.FxRate,
                    CbiCode = t.CbiCode,
                    DescriptionAr = t.DescriptionAr,
                    DescriptionEn = t.DescriptionEn,
                    BranchId = t.BranchId,
                    CreatedBy = t.CreatedBy,
                    Status = t.Status,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            return transactions;
        }
        public async Task<List<Branch>> GetAvailableBranchesAsync()
        {
            return await _context.Branches
                .Where(b => b.Status == "active")
                .OrderBy(b => b.Id)
                .Select(b => new Branch
                {
                    Id = b.Id,
                    Name_Ar = b.Name_Ar,
                    Status = b.Status
                })
                .ToListAsync();
        }

        // NEW TRANSACTION QUERY METHODS
        public async Task<IEnumerable<TransactionDetailDto>> GetTransactionsByRefAndBranchAsync(TransactionQueryRequest request)
        {
            var query = _context.GlTransactions
                .Where(t => t.Status == "completed");

            // Apply branch filter
            if (!string.IsNullOrEmpty(request.BranchId))
            {
                query = query.Where(t => t.BranchId == request.BranchId);
            }

            // Apply transaction reference filter
            if (!string.IsNullOrEmpty(request.TransactionRef))
            {
                query = query.Where(t => t.TransactionRef == request.TransactionRef);
            }

            var transactions = await query
                .OrderByDescending(t => t.Date)
                .Select(t => new TransactionDetailDto
                {
                    Id = t.Id,
                    GlId = t.GlId,
                    GlName = t.GlName,
                    TransactionRef = t.TransactionRef,
                    Date = t.Date,
                    ValueDate = t.ValueDate,
                    DebitAccount = t.DebitAccount,
                    CreditAccount = t.CreditAccount,
                    Amount = t.Amount,
                    AmountIqd = t.AmountIqd,
                    Currency = t.Currency,
                    FxRate = t.FxRate,
                    CbiCode = t.CbiCode,
                    DescriptionAr = t.DescriptionAr,
                    DescriptionEn = t.DescriptionEn,
                    BranchId = t.BranchId,
                    CreatedBy = t.CreatedBy,
                    Status = t.Status,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            return transactions;
        }

        public async Task<IEnumerable<TransactionDetailDto>> GetTransactionsByDateRangeAndBranchAsync(TransactionQueryRequest request)
        {
            var query = _context.GlTransactions
                .Where(t => t.Status == "completed");

            // Apply branch filter
            if (!string.IsNullOrEmpty(request.BranchId))
            {
                query = query.Where(t => t.BranchId == request.BranchId);
            }

            // Apply date range filter
            if (request.StartDate.HasValue)
            {
                query = query.Where(t => t.Date >= request.StartDate.Value);
            }

            if (request.EndDate.HasValue)
            {
                var endDate = request.EndDate.Value.Date.AddDays(1).AddSeconds(-1);
                query = query.Where(t => t.Date <= endDate);
            }

            var transactions = await query
                .OrderByDescending(t => t.Date)
                .Select(t => new TransactionDetailDto
                {
                    Id = t.Id,
                    GlId = t.GlId,
                    GlName = t.GlName,
                    TransactionRef = t.TransactionRef,
                    Date = t.Date,
                    ValueDate = t.ValueDate,
                    DebitAccount = t.DebitAccount,
                    CreditAccount = t.CreditAccount,
                    Amount = t.Amount,
                    AmountIqd = t.AmountIqd,
                    Currency = t.Currency,
                    FxRate = t.FxRate,
                    CbiCode = t.CbiCode,
                    DescriptionAr = t.DescriptionAr,
                    DescriptionEn = t.DescriptionEn,
                    BranchId = t.BranchId,
                    CreatedBy = t.CreatedBy,
                    Status = t.Status,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            return transactions;
        }

        public async Task<IEnumerable<TransactionDetailDto>> GetTransactionsByRefDateRangeAndBranchAsync(TransactionQueryRequest request)
        {
            var query = _context.GlTransactions
                .Where(t => t.Status == "completed");

            // Apply branch filter
            if (!string.IsNullOrEmpty(request.BranchId))
            {
                query = query.Where(t => t.BranchId == request.BranchId);
            }

            // Apply transaction reference filter
            if (!string.IsNullOrEmpty(request.TransactionRef))
            {
                query = query.Where(t => t.TransactionRef == request.TransactionRef);
            }

            // Apply date range filter
            if (request.StartDate.HasValue)
            {
                query = query.Where(t => t.Date >= request.StartDate.Value);
            }

            if (request.EndDate.HasValue)
            {
                var endDate = request.EndDate.Value.Date.AddDays(1).AddSeconds(-1);
                query = query.Where(t => t.Date <= endDate);
            }

            var transactions = await query
                .OrderByDescending(t => t.Date)
                .Select(t => new TransactionDetailDto
                {
                    Id = t.Id,
                    GlId = t.GlId,
                    GlName = t.GlName,
                    TransactionRef = t.TransactionRef,
                    Date = t.Date,
                    ValueDate = t.ValueDate,
                    DebitAccount = t.DebitAccount,
                    CreditAccount = t.CreditAccount,
                    Amount = t.Amount,
                    AmountIqd = t.AmountIqd,
                    Currency = t.Currency,
                    FxRate = t.FxRate,
                    CbiCode = t.CbiCode,
                    DescriptionAr = t.DescriptionAr,
                    DescriptionEn = t.DescriptionEn,
                    BranchId = t.BranchId,
                    CreatedBy = t.CreatedBy,
                    Status = t.Status,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            return transactions;
        }

        public async Task<IEnumerable<GlTreeDisplayDto>> GetBankWideFlatTreeReportAsync()
        {
            var request = new GlTreeReportRequest
            {
                BranchId = null,
                Currency = "IQD"
            };
            return await GetFlatTreeReportAsync(request);
        }

        public async Task<IEnumerable<GlTreeReportDto>> GetGlTreeReportAsync(GlTreeReportRequest request)
        {
            // 1. Get all active GL account definitions
            var accountDefinitions = await _context.GeneralLedgerAccounts
                .Where(a => a.Status != "closed")
                .Select(a => new
                {
                    Id = a.Id.ToString().Trim(),
                    a.NameAr
                })
                .ToListAsync();

            // 2. Calculate balances from transactions (with branch filter)
            var balanceQuery = _context.GlTransactions
                .Where(t => t.Status == "completed");

            if (!string.IsNullOrEmpty(request.BranchId))
            {
                balanceQuery = balanceQuery.Where(t => t.BranchId == request.BranchId);
            }

            var accountBalances = await balanceQuery
                .GroupBy(t => t.GlId.Trim())
                .Select(g => new
                {
                    GlId = g.Key,
                    OwnDebit = g.Sum(t => t.DebitAccount ?? 0),
                    OwnCredit = g.Sum(t => t.CreditAccount ?? 0),
                    OwnBalance = g.Sum(t => (t.CreditAccount ?? 0) - (t.DebitAccount ?? 0)),
                    TransactionCount = g.Count()
                })
                .ToListAsync();

            // 3. Combine account definitions with balances - FIXED: Include all accounts regardless of balance
            var combinedAccounts = accountDefinitions.Select(acc => new
            {
                acc.Id,
                acc.NameAr,
                OwnBalance = accountBalances.FirstOrDefault(b => b.GlId == acc.Id)?.OwnBalance ?? 0m,
                OwnDebit = accountBalances.FirstOrDefault(b => b.GlId == acc.Id)?.OwnDebit ?? 0m,
                OwnCredit = accountBalances.FirstOrDefault(b => b.GlId == acc.Id)?.OwnCredit ?? 0m,
                HasTransactions = accountBalances.Any(b => b.GlId == acc.Id) // FIXED: Check if account has ANY transactions
            }).ToList();

            Console.WriteLine($"=== ACCOUNT DATA ===");
            foreach (var acc in combinedAccounts.Where(a => a.HasTransactions))
            {
                Console.WriteLine($"{acc.Id} ({acc.NameAr}): Own Debit = {acc.OwnDebit}, Own Credit = {acc.OwnCredit}, Own Balance = {acc.OwnBalance}");
            }

            // 4. Build the hierarchical tree with cumulative balance roll-up
            var treeNodes = BuildGlTree(combinedAccounts, request.StartingGlId);

            return treeNodes;
        }

        public async Task<IEnumerable<GlTreeReportDto>> GetGlTreeByBranchAsync(string branchId)
        {
            var request = new GlTreeReportRequest { BranchId = branchId };
            return await GetGlTreeReportAsync(request);
        }

        public async Task<IEnumerable<GlTreeReportDto>> GetGlTreeByParentAsync(string parentGlId)
        {
            var request = new GlTreeReportRequest { StartingGlId = parentGlId };
            return await GetGlTreeReportAsync(request);
        }

        public async Task<GlTreeReportDto> GetGlNodeDetailsAsync(string glId, string branchId = null)
        {
            // FIXED: Always return account details even if balance is zero
            var request = new GlTreeReportRequest { StartingGlId = glId, BranchId = branchId };
            var result = await GetGlTreeReportAsync(request);
            return result.FirstOrDefault();
        }

        public async Task<IEnumerable<GlTreeDisplayDto>> GetFlatTreeReportAsync(GlTreeReportRequest request)
        {
            var treeData = await GetGlTreeReportAsync(request);
            var flatList = new List<GlTreeDisplayDto>();

            foreach (var node in treeData)
            {
                FlattenTree(node, flatList, 0);
            }

            return flatList.OrderBy(x => x.GlId).ToList();
        }

        #region Generic Tree Building Logic (Cumulative Bottom-Up)

        private List<GlTreeReportDto> BuildGlTree(IEnumerable<dynamic> accounts, string startingGlId = null)
        {
            if (!accounts.Any())
                return new List<GlTreeReportDto>();

            // Create all nodes with their OWN balances
            var allNodes = accounts.Select(a => new GlTreeReportDto
            {
                GlId = a.Id,
                GlName = a.NameAr,
                Balance = a.OwnBalance,
                AvailableBalance = a.OwnBalance,
                Level = GetGlLevel(a.Id),
                ParentGlId = GetParentGlId(a.Id),
                HierarchyPath = BuildHierarchyPath(a.Id),
                Children = new List<GlTreeReportDto>(),
                ChildCount = 0,
                OwnBalance = a.OwnBalance,
                OwnDebit = a.OwnDebit,
                OwnCredit = a.OwnCredit,
                Debit = a.OwnDebit,
                Credit = a.OwnCredit,
                HasTransactions = a.HasTransactions
            }).ToList();

            // Build dictionary for quick lookup
            var nodeDict = allNodes.ToDictionary(n => n.GlId);

            // Build parent-child relationships
            foreach (var node in allNodes)
            {
                if (string.IsNullOrEmpty(node.ParentGlId) || !nodeDict.ContainsKey(node.ParentGlId))
                {
                    // This is a root node
                }
                else
                {
                    nodeDict[node.ParentGlId].Children.Add(node);
                    nodeDict[node.ParentGlId].ChildCount++;
                }
            }

            Console.WriteLine("=== CUMULATIVE BALANCE ROLL-UP PROCESS ===");

            // CUMULATIVE BOTTOM-UP BALANCE ROLL-UP
            var maxLevel = allNodes.Max(n => n.Level);

            for (int level = maxLevel; level >= 1; level--)
            {
                var nodesAtLevel = allNodes.Where(n => n.Level == level).ToList();
                Console.WriteLine($"Processing level {level}: {nodesAtLevel.Count} nodes");

                foreach (var node in nodesAtLevel)
                {
                    var childrenSum = node.Children.Sum(c => c.Balance);
                    var childrenDebitSum = node.Children.Sum(c => c.Debit);
                    var childrenCreditSum = node.Children.Sum(c => c.Credit);
                    var oldBalance = node.Balance;

                    // NEW BALANCE = OWN BALANCE + SUM OF CHILDREN'S CUMULATIVE BALANCES
                    node.Balance = node.OwnBalance + childrenSum;
                    node.AvailableBalance = node.OwnBalance + childrenSum;

                    // Cumulative debit and credit roll-up
                    node.Debit = node.OwnDebit + childrenDebitSum;
                    node.Credit = node.OwnCredit + childrenCreditSum;

                    Console.WriteLine($"  {node.GlId} ({node.GlName}):");
                    Console.WriteLine($"    Own Debit: {node.OwnDebit}, Own Credit: {node.OwnCredit}");
                    Console.WriteLine($"    Children Debit: {childrenDebitSum}, Children Credit: {childrenCreditSum}");
                    Console.WriteLine($"    Total Debit: {node.Debit}, Total Credit: {node.Credit}");
                    Console.WriteLine($"    Own Balance: {node.OwnBalance}");
                    Console.WriteLine($"    Children sum: {childrenSum}");
                    Console.WriteLine($"    Total Balance: {node.OwnBalance} + {childrenSum} = {node.Balance}");
                }
            }

            // FIXED: Remove filtering - include ALL accounts regardless of balance
            // This ensures buttons work even when credit = debit
            var filteredNodes = allNodes; // Include all nodes

            // Return specific node or all filtered root nodes
            if (!string.IsNullOrEmpty(startingGlId) && nodeDict.ContainsKey(startingGlId))
            {
                var specificNode = nodeDict[startingGlId];
                return new List<GlTreeReportDto> { specificNode };
            }

            // Return only root nodes
            return filteredNodes.Where(n => string.IsNullOrEmpty(n.ParentGlId) || !nodeDict.ContainsKey(n.ParentGlId)).ToList();
        }

        private int GetGlLevel(string glId)
        {
            if (string.IsNullOrEmpty(glId)) return 0;
            return glId.Length;
        }

        private string GetParentGlId(string glId)
        {
            if (string.IsNullOrEmpty(glId) || glId.Length <= 1)
                return null;

            return glId.Substring(0, glId.Length - 1);
        }

        private string BuildHierarchyPath(string glId)
        {
            var pathParts = new List<string>();
            var currentId = glId;

            while (!string.IsNullOrEmpty(currentId))
            {
                pathParts.Insert(0, currentId);
                currentId = GetParentGlId(currentId);
            }

            return string.Join(" → ", pathParts);
        }

        #endregion

        #region Flat List Display

        private void FlattenTree(GlTreeReportDto node, List<GlTreeDisplayDto> flatList, int depth)
        {
            var displayNode = new GlTreeDisplayDto
            {
                GlId = node.GlId,
                GlName = node.GlName,
                LevelName = GetLevelName(node.Level),
                Balance = node.Balance,
                AvailableBalance = node.AvailableBalance,
                OwnBalance = node.OwnBalance,
                Debit = node.Debit,
                Credit = node.Credit,
                OwnDebit = node.OwnDebit,
                OwnCredit = node.OwnCredit,
                ParentGlId = node.ParentGlId,
                Depth = depth,
                HasChildren = node.ChildCount > 0,
                FullPath = node.HierarchyPath,
                IsLeafNode = node.ChildCount == 0
            };

            flatList.Add(displayNode);

            foreach (var child in node.Children.OrderBy(c => c.GlId))
            {
                FlattenTree(child, flatList, depth + 1);
            }
        }

        private string GetLevelName(int level)
        {
            return level switch
            {
                1 => "Main Category",
                2 => "Group",
                3 => "Subgroup",
                4 => "Account Type",
                5 => "Currency/Detail",
                6 => "Branch Specific",
                _ => $"Level {level}"
            };
        }

        #endregion
    }
}