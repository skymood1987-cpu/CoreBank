// Models/GlTransaction.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MinCoreBank.Models
{
    [Table("transactions")]
    public class GlTransaction
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("gl_id", TypeName = "varchar(20)")]
        public string? GlId { get; set; }

        [Column("gl_name", TypeName = "nvarchar(100)")]
        public string? GlName { get; set; }

        [Column("transaction_ref", TypeName = "char(16)")]
        public string? TransactionRef { get; set; }

       // [Required]
        [Column("date", TypeName = "date")]
        public DateTime? Date { get; set; }

     //   [Required]
        [Column("value_date", TypeName = "date")]
        public DateTime? ValueDate { get; set; }

        //[Required]
        [Column("debit_account", TypeName = "decimal(18,2)")]
        public decimal? DebitAccount { get; set; }

        [Column("credit_account", TypeName = "decimal(18,2)")]
        public decimal? CreditAccount { get; set; }

        //   [Required]
        [Column("amount", TypeName = "decimal?(18, 2)")]
        public decimal? Amount { get; set; }

      //  [Required]
        [Column("amount_iqd", TypeName = "decimal?(18, 2)")]
        public decimal? AmountIqd { get; set; }

     //   [Required]
        [Column("currency", TypeName = "char(3)")]
        public string? Currency { get; set; }

        [Column("fx_rate", TypeName = "decimal?(9, 4)")]
        public decimal? FxRate { get; set; } = 0.0m;

        [Column("cbi_code", TypeName = "char(10)")]
        public string? CbiCode { get; set; }

        [Column("description_ar", TypeName = "varchar(100)")]
        public string? DescriptionAr { get; set; }

        [Column("description_en", TypeName = "varchar(100)")]
        public string? DescriptionEn { get; set; }

        [Column("branch_id", TypeName = "char(5)")]
        public string? BranchId { get; set; }

        [Column("created_by", TypeName = "varchar(20)")]
        public string? CreatedBy { get; set; }

        [Column("status", TypeName = "varchar(10)")]
        public string? Status { get; set; } = "completed";

        [Column("reversal_ref")]
        public long? ReversalRef { get; set; }

        [Column("created_at", TypeName = "datetime")]
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at", TypeName = "datetime")]
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_by", TypeName = "varchar(20)")]
        public string? UpdatedBy { get; set; }
    }
}