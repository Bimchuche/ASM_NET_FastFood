using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASM1_NET.Migrations
{
    /// <inheritdoc />
    public partial class AddPayOSFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentLinkId",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PaymentOrderCode",
                table: "Orders",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentLinkId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaymentOrderCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "Orders");
        }
    }
}
