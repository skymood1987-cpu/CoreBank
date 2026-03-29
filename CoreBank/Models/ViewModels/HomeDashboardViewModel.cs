namespace MinCoreBank.Models.ViewModels
{
    public class DailyTransactionCountPoint
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }

    public class HomeDashboardViewModel
    {
        public string BranchId { get; set; } = string.Empty;
        public DateTime BusinessDate { get; set; }
        public decimal TotalDebitToday { get; set; }
        public decimal TotalCreditToday { get; set; }
        public int TransactionsCountToday { get; set; }
        public int ReversedCountToday { get; set; }
        public decimal DailyDifference { get; set; }
        public decimal BranchTotalBalance { get; set; }
        public decimal BranchNetBalance { get; set; }
        public decimal DailyMovementAmount { get; set; }
        public decimal DailyMovementToBranchBalancePct { get; set; }
        public int AccountsCount { get; set; }
        public bool HasBranchBalanceData { get; set; }
        public decimal CompletionRateToday { get; set; }
        public decimal AvgTransactionValueToday { get; set; }
        public decimal MonthTotalDebit { get; set; }
        public decimal MonthTotalCredit { get; set; }
        public decimal MonthMovementTotal { get; set; }
        public int ActiveDaysThisMonth { get; set; }
        public int ClosedDaysThisMonth { get; set; }
        public decimal ClosureRateThisMonth { get; set; }
        public DateTime? LastClosedDate { get; set; }
        public decimal LastClosedDebit { get; set; }
        public decimal LastClosedCredit { get; set; }
        public List<DailyTransactionCountPoint> DailyTransactionCounts { get; set; } = new();
        public int MaxDailyTransactionCount { get; set; }
        public bool IsBalanced => DailyDifference == 0;
        public bool IsDayClosed { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ClosedAtUtc { get; set; }
    }
}
