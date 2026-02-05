// Models/Dtos/GlTransactionCreateDto.cs
using MinCoreBank.Models.Utils;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace MinCoreBank.Models.Dtos
{
    public class GlTransactionCreateDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Let the database generate the TransactionID
        public int? id { get; set; }
        public string? GlName { get; set; }
        public string? GlId { get; set; }

        // [Required]
        public string? TransactionRef { get; set; }

       // [Required]
        public DateTime? Date { get; set; }

     //   [Required]
        public DateTime? ValueDate { get; set; }

     //   [Required]
        public decimal? DebitAccount { get; set; }

    //    [Required]
        public decimal? CreditAccount { get; set; }

      //  [Required]
        [Range(0.01, double.MaxValue)]
        public decimal? Amount { get; set; }

       // [Required]
        [StringLength(3)]
        public string? Currency { get; set; }

        public decimal FxRate { get; set; } = 0.0m;

        public string? CbiCode { get; set; }
        public string? DescriptionAr { get; set; }
        public string? DescriptionEn { get; set; }
        public string? BranchId { get; set; }

        public void GenerateTempReference()
        {
            TransactionRef = BinderNumberGenerator.GenerateTempReference();
        }



    }

    public class GlTransactionUpdateDto
    {
        public string? Status { get; set; }
        public string? DescriptionAr { get; set; }
        public decimal? DebitAccount { get; set; }  
        public decimal? CreditAccount { get; set; }
        public decimal? Amount { get; set; }
    }

    public class GlTransactionResponseDto
    {
        public long Id { get; set; }
        public string? GlId { get; set; }
        public string? GlName { get; set; }
        public string? TransactionRef { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? ValueDate { get; set; }
        public decimal? DebitAccount { get; set; }
        public decimal? CreditAccount { get; set; }
        public decimal? Amount { get; set; }
        public decimal? AmountIqd { get; set; }
        public string? Currency { get; set; }
        public decimal? FxRate { get; set; }
        public string? CbiCode { get; set; }
        public string? DescriptionAr { get; set; }
        public string? DescriptionEn { get; set; }
        public string? BranchId { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

}
  