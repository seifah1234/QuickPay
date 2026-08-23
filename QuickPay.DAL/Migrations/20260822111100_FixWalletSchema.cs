using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuickPay.DAL.Migrations
{
    /// <inheritdoc />
    public partial class FixWalletSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinancialAccounts_Users_UserId",
                table: "FinancialAccounts");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialAccounts_Users_Wallet_UserId",
                table: "FinancialAccounts");

            migrationBuilder.DropIndex(
                name: "IX_FinancialAccounts_Wallet_UserId",
                table: "FinancialAccounts");

            migrationBuilder.DropColumn(
                name: "Wallet_UserId",
                table: "FinancialAccounts");

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialAccounts_Users_UserId",
                table: "FinancialAccounts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinancialAccounts_Users_UserId",
                table: "FinancialAccounts");

            migrationBuilder.AddColumn<int>(
                name: "Wallet_UserId",
                table: "FinancialAccounts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialAccounts_Wallet_UserId",
                table: "FinancialAccounts",
                column: "Wallet_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialAccounts_Users_UserId",
                table: "FinancialAccounts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialAccounts_Users_Wallet_UserId",
                table: "FinancialAccounts",
                column: "Wallet_UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
