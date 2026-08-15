using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuickPay.DAL.Migrations
{
    /// <inheritdoc />
    public partial class EditFinancialAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinancialAccounts_Users_OwnerId",
                table: "FinancialAccounts");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialAccounts_Users_UserId",
                table: "FinancialAccounts");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "FinancialAccounts",
                newName: "Wallet_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_FinancialAccounts_OwnerId",
                table: "FinancialAccounts",
                newName: "IX_FinancialAccounts_Wallet_UserId");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "FinancialAccounts",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinancialAccounts_Users_UserId",
                table: "FinancialAccounts");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialAccounts_Users_Wallet_UserId",
                table: "FinancialAccounts");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "FinancialAccounts");

            migrationBuilder.RenameColumn(
                name: "Wallet_UserId",
                table: "FinancialAccounts",
                newName: "OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_FinancialAccounts_Wallet_UserId",
                table: "FinancialAccounts",
                newName: "IX_FinancialAccounts_OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialAccounts_Users_OwnerId",
                table: "FinancialAccounts",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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
