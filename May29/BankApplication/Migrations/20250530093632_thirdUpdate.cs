using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankApplication.Migrations
{
    /// <inheritdoc />
    public partial class thirdUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DepositsByAccount",
                columns: table => new
                {
                    BankTransactionId = table.Column<int>(type: "integer", nullable: false),
                    AmountTransferred = table.Column<decimal>(type: "numeric", nullable: false),
                    TransferDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "ReceivedBankTransactionsByAccount",
                columns: table => new
                {
                    BankTransactionId = table.Column<int>(type: "integer", nullable: false),
                    SenderId = table.Column<int>(type: "integer", nullable: false),
                    AmountTransferred = table.Column<decimal>(type: "numeric", nullable: false),
                    TransferDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "SentBankTransactionsByAccount",
                columns: table => new
                {
                    BankTransactionId = table.Column<int>(type: "integer", nullable: false),
                    ReceiverId = table.Column<int>(type: "integer", nullable: false),
                    AmountTransferred = table.Column<decimal>(type: "numeric", nullable: false),
                    TransferDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "WithdrawalsByAccount",
                columns: table => new
                {
                    BankTransactionId = table.Column<int>(type: "integer", nullable: false),
                    AmountTransferred = table.Column<decimal>(type: "numeric", nullable: false),
                    TransferDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DepositsByAccount");

            migrationBuilder.DropTable(
                name: "ReceivedBankTransactionsByAccount");

            migrationBuilder.DropTable(
                name: "SentBankTransactionsByAccount");

            migrationBuilder.DropTable(
                name: "WithdrawalsByAccount");
        }
    }
}
