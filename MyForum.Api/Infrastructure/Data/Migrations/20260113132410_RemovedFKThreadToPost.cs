using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyForum.Api.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemovedFKThreadToPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Threads_Posts_OriginalPostId",
                table: "Threads");

            migrationBuilder.DropIndex(
                name: "IX_Threads_LastBumpAt",
                table: "Threads");

            migrationBuilder.DropIndex(
                name: "IX_Threads_OriginalPostId",
                table: "Threads");

            migrationBuilder.DropColumn(
                name: "OriginalPostId",
                table: "Threads");

            migrationBuilder.DropColumn(
                name: "AuthorId",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "AuthorTripcode",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "PostPassword",
                table: "Posts");

            migrationBuilder.AlterColumn<string>(
                name: "UserAgent",
                table: "Posts",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Posts",
                type: "character varying(1500)",
                maxLength: 1500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AddColumn<string>(
                name: "IpAddressHash",
                table: "Posts",
                type: "character varying(44)",
                maxLength: 44,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsOriginal",
                table: "Posts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Boards",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "NOW()");

            migrationBuilder.CreateIndex(
                name: "IX_Threads_IsPinned_LastBumpAt",
                table: "Threads",
                columns: new[] { "IsPinned", "LastBumpAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Posts_ThreadId_IsOriginal",
                table: "Posts",
                columns: new[] { "ThreadId", "IsOriginal" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Threads_IsPinned_LastBumpAt",
                table: "Threads");

            migrationBuilder.DropIndex(
                name: "IX_Posts_ThreadId_IsOriginal",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "IpAddressHash",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "IsOriginal",
                table: "Posts");

            migrationBuilder.AddColumn<int>(
                name: "OriginalPostId",
                table: "Threads",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserAgent",
                table: "Posts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Posts",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1500)",
                oldMaxLength: 1500);

            migrationBuilder.AddColumn<string>(
                name: "AuthorId",
                table: "Posts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthorTripcode",
                table: "Posts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "Posts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostPassword",
                table: "Posts",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Boards",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateIndex(
                name: "IX_Threads_LastBumpAt",
                table: "Threads",
                column: "LastBumpAt");

            migrationBuilder.CreateIndex(
                name: "IX_Threads_OriginalPostId",
                table: "Threads",
                column: "OriginalPostId");

            migrationBuilder.AddForeignKey(
                name: "FK_Threads_Posts_OriginalPostId",
                table: "Threads",
                column: "OriginalPostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
