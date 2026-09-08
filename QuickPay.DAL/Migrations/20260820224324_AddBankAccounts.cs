using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuickPay.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddBankAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BankAccountId",
                table: "PaymentGatewayTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Direction",
                table: "PaymentGatewayTransactions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "PaymentGatewayTransactions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WalletId",
                table: "PaymentGatewayTransactions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "BankAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MaskedNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    GatewayToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankAccounts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentGatewayTransactions_BankAccountId",
                table: "PaymentGatewayTransactions",
                column: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentGatewayTransactions_UserId",
                table: "PaymentGatewayTransactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentGatewayTransactions_WalletId",
                table: "PaymentGatewayTransactions",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_UserId",
                table: "BankAccounts",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentGatewayTransactions_BankAccounts_BankAccountId",
                table: "PaymentGatewayTransactions",
                column: "BankAccountId",
                principalTable: "BankAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentGatewayTransactions_FinancialAccounts_WalletId",
                table: "PaymentGatewayTransactions",
                column: "WalletId",
                principalTable: "FinancialAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentGatewayTransactions_Users_UserId",
                table: "PaymentGatewayTransactions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentGatewayTransactions_BankAccounts_BankAccountId",
                table: "PaymentGatewayTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentGatewayTransactions_FinancialAccounts_WalletId",
                table: "PaymentGatewayTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentGatewayTransactions_Users_UserId",
                table: "PaymentGatewayTransactions");

            migrationBuilder.DropTable(
                name: "BankAccounts");

            migrationBuilder.DropIndex(
                name: "IX_PaymentGatewayTransactions_BankAccountId",
                table: "PaymentGatewayTransactions");

            migrationBuilder.DropIndex(
                name: "IX_PaymentGatewayTransactions_UserId",
                table: "PaymentGatewayTransactions");

            migrationBuilder.DropIndex(
                name: "IX_PaymentGatewayTransactions_WalletId",
                table: "PaymentGatewayTransactions");

            migrationBuilder.DropColumn(
                name: "BankAccountId",
                table: "PaymentGatewayTransactions");

            migrationBuilder.DropColumn(
                name: "Direction",
                table: "PaymentGatewayTransactions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "PaymentGatewayTransactions");

            migrationBuilder.DropColumn(
                name: "WalletId",
                table: "PaymentGatewayTransactions");
        }
    }
}
