using ExpenseTrackingSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTrackingSystem.Contexts
{
    public class ExpenseContext : DbContext
    {
        public ExpenseContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Receipt> Receipts { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasKey(u => u.Username)
                .HasName("PK_User");

            modelBuilder.Entity<Expense>().HasOne(e => e.User)
                .WithMany(u => u.Expenses)
                .HasForeignKey(e => e.Username)
                .HasConstraintName("FK_Expense_User")
                .OnDelete(DeleteBehavior.Cascade); 

            modelBuilder.Entity<Receipt>().HasOne(r => r.User)
                .WithMany(u => u.Receipts)
                .HasForeignKey(r => r.Username)
                .HasConstraintName("FK_Receipt_User")
                .OnDelete(DeleteBehavior.Cascade); 

            modelBuilder.Entity<Receipt>().HasOne(r => r.Expense)
                .WithOne(e => e.Receipt)
                .HasForeignKey<Receipt>(r => r.ExpenseId)
                .HasConstraintName("FK_Receipt_Expense")
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AuditLog>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(a => a.Username)
                .HasConstraintName("FK_AuditLog_User")
                .OnDelete(DeleteBehavior.Restrict); 


            modelBuilder.Entity<Expense>()
                .Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Receipt>()
                .Property(r => r.Username)
                .IsRequired()
                .HasMaxLength(100);
        }
    }
}