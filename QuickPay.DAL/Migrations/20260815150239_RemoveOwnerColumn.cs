using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuickPay.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOwnerColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SharedWalletName",
                table: "FinancialAccounts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SharedWalletName",
                table: "FinancialAccounts");
        }
    }
}
