using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MinCoreBank.Models
{
    [Table("accounts")]
    public class GeneralLedgerAccount
    {
        [Key]
        [Column("id", TypeName = "int")]
        public int Id { get; set; }

        [Required]
        [Column("name_ar", TypeName = "varchar(50)")]
        public string NameAr { get; set; }

        [Column("name_en", TypeName = "varchar(50)")]
        public string? NameEn { get; set; }

        [Required]
        [Column("type", TypeName = "char(1)")]
        public string Type { get; set; }

        [Column("subtype", TypeName = "varchar(20)")]
        public string? Subtype { get; set; }

        [Required]
        [Column("currency", TypeName = "char(3)")]
        public string Currency { get; set; }

        [Required]
        [Column("branch_id", TypeName = "char(5)")]
        public string BranchId { get; set; }

        [Column("customer_id", TypeName = "int")]
        public int? CustomerId { get; set; }

        [Required]
        [Column("balance", TypeName = "decimal(18, 2)")]
        public decimal Balance { get; set; }

        [Required]
        [Column("available_balance", TypeName = "decimal(18, 2)")]
        public decimal AvailableBalance { get; set; }

        [Required]
        [Column("opening_date", TypeName = "date")]
        public DateTime OpeningDate { get; set; }

        [Column("last_activity_date", TypeName = "date")]
        public DateTime? LastActivityDate { get; set; }

        [Required]
        [Column("status", TypeName = "varchar(10)")]
        public string Status { get; set; }

        [Column("interest_rate", TypeName = "decimal(5, 2)")]
        public decimal? InterestRate { get; set; }

        [Column("created_at", TypeName = "datetime")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at", TypeName = "datetime")]
        public DateTime? UpdatedAt { get; set; }

        [Column("updated_by", TypeName = "varchar(20)")]
        public string? UpdatedBy { get; set; }

        // Navigation property
       
    }
}