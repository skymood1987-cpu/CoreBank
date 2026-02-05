using System;
using System.Collections.Generic;

namespace MinCoreBank.Models.Dtos
{
   

    public class GlTreeReportRequest
    {
        public string BranchId { get; set; }
        public DateTime? AsOfDate { get; set; }
        public string Currency { get; set; } = "IQD";
        public string StartingGlId { get; set; } // Start from specific parent node
        public bool IncludeChildren { get; set; } = true;
    }

    public class GlTreeDisplayDto
    {
        public string GlId { get; set; }
        public string GlName { get; set; }
        public string LevelName { get; set; }
        public decimal Balance { get; set; }           // Cumulative total
        public decimal AvailableBalance { get; set; }  // Cumulative total
        public decimal OwnBalance { get; set; }        // This account's own amount
        public string ParentGlId { get; set; }
        public string ParentGlName { get; set; }
        public string BranchId { get; set; }
        public int Depth { get; set; }
        public bool HasChildren { get; set; }
        public bool IsLeafNode { get; set; }
        public string FullPath { get; set; }

        public decimal Debit { get; set; }           // Cumulative debit
        public decimal Credit { get; set; }          // Cumulative credit
        public decimal OwnDebit { get; set; }        // This account's own debit only
        public decimal OwnCredit { get; set; }

        public class TransactionQueryRequest
        {
            public string BranchId { get; set; }
            public string TransactionRef { get; set; }
            public string GlId { get; set; } // ADD THIS
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public string Currency { get; set; } = "IQD";
        }

        public class TransactionDetailDto
        {
            public long Id { get; set; }
            public string GlId { get; set; }
            public string GlName { get; set; }
            public string TransactionRef { get; set; }
            public DateTime? Date { get; set; }
            public DateTime? ValueDate { get; set; }
            public decimal? DebitAccount { get; set; }
            public decimal? CreditAccount { get; set; }
            public decimal? Amount { get; set; }
            public decimal? AmountIqd { get; set; }
            public string? Currency { get; set; }
            public decimal? FxRate { get; set; }
            public string CbiCode { get; set; }
            public string DescriptionAr { get; set; }
            public string DescriptionEn { get; set; }
            public string BranchId { get; set; }
            public string CreatedBy { get; set; }
            public string? Status { get; set; }
            public DateTime? CreatedAt { get; set; }
        }
        public class GlTreeReportDto
        {
            public string GlId { get; set; }
            public string GlName { get; set; }
            public int Level { get; set; }
            public decimal Balance { get; set; }           // Cumulative: Own + Children
            public decimal AvailableBalance { get; set; }  // Cumulative: Own + Children  
            public decimal OwnBalance { get; set; }        // This account's own transactions only
            public string ParentGlId { get; set; }
            public string BranchId { get; set; }
            public int ChildCount { get; set; }
            public string HierarchyPath { get; set; }
            public bool HasTransactions { get; set; }


            public decimal Debit { get; set; }           // Cumulative: Own + Children
            public decimal Credit { get; set; }          // Cumulative: Own + Children
            public decimal OwnDebit { get; set; }        // This account's own debit only
            public decimal OwnCredit { get; set; }       // This account's own credit only
              public List<GlTreeReportDto> Children { get; set; } = new List<GlTreeReportDto>();


        }
    }
}