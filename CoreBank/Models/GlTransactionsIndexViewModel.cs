using System;
using System.Collections.Generic;

namespace MinCoreBank.Models
{
    public class GlTransactionsIndexViewModel
    {
        public IEnumerable<GlTransaction> Transactions { get; set; } = Array.Empty<GlTransaction>();
        public string Period { get; set; } = "today";
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal TotalDebit { get; set; }
        public string BalanceStatus { get; set; } = string.Empty;
    }
}
