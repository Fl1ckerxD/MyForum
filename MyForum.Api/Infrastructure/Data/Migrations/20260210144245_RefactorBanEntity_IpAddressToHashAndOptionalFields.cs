using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyForum.Api.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RefactorBanEntity_IpAddressToHashAndOptionalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bans_ExpiresAt",
                table: "Bans");

            migrationBuilder.DropIndex(
                name: "IX_Bans_IpAddress",
                table: "Bans");

            migrationBuilder.DropIndex(
                name: "IX_Bans_IsActive",
                table: "Bans");

            migrationBuilder.RenameColumn(
                name: "IpAddress",
                table: "Bans",
                newName: "IpAddressHash");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpiresAt",
                table: "Bans",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateIndex(
                name: "IX_Bans_IpAddressHash_BoardId",
                table: "Bans",
                columns: new[] { "IpAddressHash", "BoardId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bans_IpAddressHash_BoardId",
                table: "Bans");

            migrationBuilder.RenameColumn(
                name: "IpAddressHash",
                table: "Bans",
                newName: "IpAddress");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpiresAt",
                table: "Bans",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bans_ExpiresAt",
                table: "Bans",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_Bans_IpAddress",
                table: "Bans",
                column: "IpAddress");

            migrationBuilder.CreateIndex(
                name: "IX_Bans_IsActive",
                table: "Bans",
                column: "IsActive");
        }
    }
}
