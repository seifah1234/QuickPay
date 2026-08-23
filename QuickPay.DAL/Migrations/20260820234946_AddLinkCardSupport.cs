using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuickPay.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddLinkCardSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "WalletId",
                table: "PaymentGatewayTransactions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "ProviderTransactionId",
                table: "PaymentGatewayTransactions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProviderTransactionId",
                table: "PaymentGatewayTransactions");

            migrationBuilder.AlterColumn<int>(
                name: "WalletId",
                table: "PaymentGatewayTransactions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
