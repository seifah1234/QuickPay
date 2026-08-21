using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuickPay.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddProviderOrderId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProviderOrderId",
                table: "PaymentGatewayTransactions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentGatewayTransactions_ProviderOrderId",
                table: "PaymentGatewayTransactions",
                column: "ProviderOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentGatewayTransactions_ProviderOrderId",
                table: "PaymentGatewayTransactions");

            migrationBuilder.DropColumn(
                name: "ProviderOrderId",
                table: "PaymentGatewayTransactions");
        }
    }
}
