// Models/DailyBranchApproval.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MinCoreBank.Models
{
    [Table("daily_branch_approvals")]
    public class DailyBranchApproval
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("branch_id", TypeName = "varchar(5)")]
        public string BranchId { get; set; } = string.Empty;

        [Required]
        [Column("approval_date")]
        public DateTime ApprovalDate { get; set; }

        [Required]
        [Column("approved_by", TypeName = "varchar(100)")]
        public string ApprovedBy { get; set; } = string.Empty;

        [Column("is_locked")]
        public bool IsLocked { get; set; } = true;

        [Column("locked_at")]
        public DateTime LockedAt { get; set; } = DateTime.UtcNow;

        [Column("total_credit", TypeName = "decimal(18,2)")]
        public decimal TotalCredit { get; set; }

        [Column("total_debit", TypeName = "decimal(18,2)")]
        public decimal TotalDebit { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}