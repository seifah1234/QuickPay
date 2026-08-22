using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuickPay.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddSmartSplitSettlementWallet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinancialAccounts_Users_UserId",
                table: "FinancialAccounts");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DueDate",
                table: "SplitGroups",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<int>(
                name: "SettlementWalletId",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Wallet_UserId",
                table: "FinancialAccounts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_SettlementWalletId",
                table: "Payments",
                column: "SettlementWalletId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_FinancialAccounts_SettlementWalletId",
                table: "Payments",
                column: "SettlementWalletId",
                principalTable: "FinancialAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinancialAccounts_Users_UserId",
                table: "FinancialAccounts");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialAccounts_Users_Wallet_UserId",
                table: "FinancialAccounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_FinancialAccounts_SettlementWalletId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_SettlementWalletId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_FinancialAccounts_Wallet_UserId",
                table: "FinancialAccounts");

            migrationBuilder.DropColumn(
                name: "SettlementWalletId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Wallet_UserId",
                table: "FinancialAccounts");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DueDate",
                table: "SplitGroups",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialAccounts_Users_UserId",
                table: "FinancialAccounts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
