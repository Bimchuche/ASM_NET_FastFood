using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASM1_NET.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordChangeOTP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NewPasswordPending",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordChangeOTP",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordChangeOTPExpiry",
                table: "Users",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NewPasswordPending",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordChangeOTP",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordChangeOTPExpiry",
                table: "Users");
        }
    }
}
