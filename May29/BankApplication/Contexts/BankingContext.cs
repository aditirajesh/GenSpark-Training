using BankApplication.Models;
using BankApplication.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BankApplication.Contexts
{
    public class BankingContext : DbContext
    {
        public BankingContext(DbContextOptions<BankingContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<BankTransaction> BankTransactions { get; set; }

        public DbSet<SentBankTransactionResponseDto> SentBankTransactionsByAccount { get; set; }
        public DbSet<ReceivedBankTransactionResponseDto> ReceivedBankTransactionsByAccount { get; set;}
        public DbSet<DepositResponseDto>DepositsByAccount { get; set; }
        public DbSet<WithdrawalResponseDto>WithdrawalsByAccount { get; set;}

        public async Task<List<SentBankTransactionResponseDto>> GetSentBankTransactionsByAccount(int account_id)
        {
            return await this.Set<SentBankTransactionResponseDto>()
                            .FromSqlInterpolated($"SELECT * from fn_GetSentBankTransactions({account_id})")
                            .ToListAsync();
        }

        public async Task<List<ReceivedBankTransactionResponseDto>> GetReceivedBankTransactionsByAccount(int account_id)
        {
            return await this.Set<ReceivedBankTransactionResponseDto>()
                            .FromSqlInterpolated($"SELECT * from fn_GetReceivedBankTransactions({account_id})")
                            .ToListAsync();
        }

        public async Task<List<DepositResponseDto>> GetDepositsByAccount(int account_id)
        {
            return await this.Set<DepositResponseDto>()
                            .FromSqlInterpolated($"SELECT * from fn_GetDeposits({account_id})")
                            .ToListAsync();
        }

        public async Task<List<WithdrawalResponseDto>> GetWithdrawalsByAccount(int account_id)
        {
            return await this.Set<WithdrawalResponseDto>()
                            .FromSqlInterpolated($"SELECT * from fn_GetWithdrawals({account_id})")
                            .ToListAsync();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SentBankTransactionResponseDto>().HasNoKey();
            modelBuilder.Entity<ReceivedBankTransactionResponseDto>().HasNoKey();
            modelBuilder.Entity<DepositResponseDto>().HasNoKey();
            modelBuilder.Entity<WithdrawalResponseDto>().HasNoKey();

            modelBuilder.Entity<User>().HasKey(u => u.UserId).HasName("PK_UserId");
            modelBuilder.Entity<Account>().HasKey(a => a.AccountId).HasName("PK_AccountId");
            modelBuilder.Entity<BankTransaction>().HasKey(t => t.BankTransactionId).HasName("PK_BankTransaction_Id");

            modelBuilder.Entity<Account>().HasOne(a => a.User)
                                        .WithMany(u => u.Accounts)
                                        .HasForeignKey(a => a.UserId)
                                        .HasConstraintName("FK_Account_User")
                                        .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BankTransaction>().HasOne(t => t.Sender)
                                              .WithMany(s => s.SentBankTransactions)
                                              .HasForeignKey(t => t.SenderId)
                                              .HasConstraintName("fk_BankTransaction_SenderAcc")
                                              .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BankTransaction>().HasOne(t => t.Receiver)
                                              .WithMany(s => s.ReceivedBankTransactions)
                                              .HasForeignKey(t => t.ReceiverId)
                                              .HasConstraintName("fk_BankTransaction_ReceiverAcc")
                                              .OnDelete(DeleteBehavior.Restrict);
        }

    }
}