namespace MinCoreBank.Models.Dtos
{
    public class GeneralLedgerAccountCreateDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; }    // PascalCase
        public string NameEn { get; set; }
        public string Type { get; set; }
        public string Subtype { get; set; }
        public string Currency { get; set; }
        public string BranchId { get; set; }
        public int? CustomerId { get; set; }
        public decimal Balance { get; set; }
        public decimal AvailableBalance { get; set; }
        public DateTime OpeningDate { get; set; }
        public string Status { get; set; }
       
    }

    public class GeneralLedgerAccountUpdateDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string Status { get; set; }
        public decimal? InterestRate { get; set; }
    }

    public class GeneralLedgerAccountResponseDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string Type { get; set; }
        public string Subtype { get; set; }
        public string Currency { get; set; }
        public string BranchId { get; set; }
        public int? CustomerId { get; set; }
        public decimal Balance { get; set; }
        public decimal AvailableBalance { get; set; }
        public DateTime OpeningDate { get; set; }
        public DateTime? LastActivityDate { get; set; }
        public string Status { get; set; }
        public decimal? InterestRate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}