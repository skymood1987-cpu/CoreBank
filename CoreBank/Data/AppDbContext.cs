using Microsoft.EntityFrameworkCore;
using MinCoreBank.Models;

namespace MinCoreBank.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<DailyBranchApproval> DailyBranchApprovals { get; set; }
       
        public DbSet<Users> Users { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<GeneralLedgerAccount> GeneralLedgerAccounts { get; set; }
        public DbSet<GlTransaction> GlTransactions { get; set; }
      

        // In OnModelCreating method, ADD this configuration:
       
protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DailyBranchApproval>(entity =>
            {
                entity.ToTable("daily_branch_approvals");

                entity.HasIndex(e => new { e.BranchId, e.ApprovalDate })
                      .IsUnique()
                      .HasDatabaseName("UK_BranchDate");

                entity.Property(e => e.BranchId)
                    .HasColumnName("branch_id")
                    .HasMaxLength(5)
                    .IsRequired();

                entity.Property(e => e.ApprovalDate)
                    .HasColumnName("approval_date")
                    .IsRequired();

                entity.Property(e => e.ApprovedBy)
                    .HasColumnName("approved_by")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.IsLocked)
                    .HasColumnName("is_locked")
                    .HasDefaultValue(true);

                entity.Property(e => e.LockedAt)
                    .HasColumnName("locked_at")
                    .IsRequired();

                entity.Property(e => e.TotalCredit)
                    .HasColumnName("total_credit")
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.TotalDebit)
                    .HasColumnName("total_debit")
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("getdate()");
            });

            modelBuilder.Entity<DailyBranchApproval>(entity =>
            {
                entity.ToTable("daily_branch_approvals");

                entity.HasIndex(e => new {
                    e.BranchId,
                    e.ApprovalDate
                })
                      .IsUnique()
                      .HasDatabaseName("UK_BranchDate");

                entity.Property(e => e.BranchId)
                    .HasColumnName("branch_id")
                    .HasMaxLength(5)
                    .IsRequired();

                entity.Property(e => e.ApprovalDate)
                    .HasColumnName("approval_date")
                    .IsRequired();

                entity.Property(e => e.ApprovedBy)
                    .HasColumnName("approved_by")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.IsLocked)
                    .HasColumnName("is_locked")
                    .HasDefaultValue(true);

                entity.Property(e => e.LockedAt)
                    .HasColumnName("locked_at")
                    .IsRequired();

                entity.Property(e => e.TotalCredit)
                    .HasColumnName("total_credit")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.TotalDebit)
                    .HasColumnName("total_debit")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("getdate()");
            });

            modelBuilder.Entity<DailyBranchApproval>(entity =>
            {
                entity.ToTable("daily_branch_approvals");
                entity.HasIndex(e => new { e.BranchId, e.ApprovalDate })
                      .IsUnique()
                      .HasDatabaseName("UK_BranchDate");
            });
            modelBuilder.Entity<GeneralLedgerAccount>(entity =>
            {
                entity.ToTable("accounts");

                // Explicit column mappings
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.NameAr).HasColumnName("name_ar");
                entity.Property(e => e.NameEn).HasColumnName("name_en");
                entity.Property(e => e.Type).HasColumnName("type");
                entity.Property(e => e.Subtype).HasColumnName("subtype");
                entity.Property(e => e.Currency).HasColumnName("currency");
                entity.Property(e => e.BranchId).HasColumnName("branch_id");
                entity.Property(e => e.CustomerId).HasColumnName("customer_id");
                entity.Property(e => e.Balance).HasColumnName("balance");
                entity.Property(e => e.AvailableBalance).HasColumnName("available_balance");
                entity.Property(e => e.OpeningDate).HasColumnName("opening_date");
                entity.Property(e => e.LastActivityDate).HasColumnName("last_activity_date");
                entity.Property(e => e.Status).HasColumnName("status");
                entity.Property(e => e.InterestRate).HasColumnName("interest_rate");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
                entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
            });

            modelBuilder.Entity<Users>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Id).HasMaxLength(20);
                entity.Property(u => u.Name_ar).IsRequired().HasMaxLength(50);
                entity.Property(u => u.Name_en).HasMaxLength(50);
                entity.Property(u => u.Role).IsRequired().HasMaxLength(10);
                entity.Property(u => u.BranchId).IsRequired().HasMaxLength(3);
                entity.Property(u => u.password_hash).IsRequired().HasMaxLength(255);
                entity.Property(u => u.Status).IsRequired().HasMaxLength(10);
            });

            // Configure GlTransaction entity
            modelBuilder.Entity<GlTransaction>(entity =>
            {
                entity.ToTable("transactions"); // Match your actual table name

                // Configure properties to match database schema
                entity.Property(t => t.GlId)
                    .HasColumnName("gl_id")
                    .HasMaxLength(20);

                entity.Property(t => t.GlName)
                    .HasColumnName("gl_name")
                    .HasMaxLength(100);

                entity.Property(t => t.TransactionRef)
                    .HasColumnName("transaction_ref")
                    .HasMaxLength(16);

                entity.Property(t => t.Date)
                    .HasColumnName("date")
                    .HasColumnType("date");

                entity.Property(t => t.ValueDate)
                    .HasColumnName("value_date")
                    .HasColumnType("date");

                entity.Property(t => t.DebitAccount)
                    .HasColumnName("debit_account")
                    .HasMaxLength(10)
                    .IsRequired();

                entity.Property(t => t.CreditAccount)
                    .HasColumnName("credit_account")
                    .HasMaxLength(10)
                    .IsRequired();

                entity.Property(t => t.Amount)
                    .HasColumnName("amount")
                    .HasColumnType("decimal(18,2)");

                entity.Property(t => t.AmountIqd)
                    .HasColumnName("amount_iqd")
                    .HasColumnType("decimal(18,2)");

                entity.Property(t => t.Currency)
                    .HasColumnName("currency")
                    .HasMaxLength(3)
                    .IsRequired();

                entity.Property(t => t.FxRate)
                    .HasColumnName("fx_rate")
                    .HasColumnType("decimal(9,4)")
                    .HasDefaultValue(1.0m);

                entity.Property(t => t.CbiCode)
                    .HasColumnName("cbi_code")
                    .HasMaxLength(10);

                entity.Property(t => t.DescriptionAr)
                    .HasColumnName("description_ar")
                    .HasMaxLength(100);

                entity.Property(t => t.DescriptionEn)
                    .HasColumnName("description_en")
                    .HasMaxLength(100);

                entity.Property(t => t.BranchId)
                    .HasColumnName("branch_id")
                    .HasMaxLength(3);

                entity.Property(t => t.CreatedBy)
                    .HasColumnName("created_by")
                    .HasMaxLength(20);

                entity.Property(t => t.Status)
                    .HasColumnName("status")
                    .HasMaxLength(10)
                    .HasDefaultValue("completed");

                entity.Property(t => t.ReversalRef)
                    .HasColumnName("reversal_ref");

                entity.Property(t => t.CreatedAt)
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("getdate()");

                entity.Property(t => t.UpdatedAt)
                    .HasColumnName("updated_at")
                    .HasDefaultValueSql("getdate()");

                entity.Property(t => t.UpdatedBy)
                    .HasColumnName("updated_by")
                    .HasMaxLength(20);
            });

            // Configure DailyGlApproval entity - ADD THIS SECTION
           
        }
    }
}