using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyForum.Api.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BoardModerators_Boards_BoardId",
                table: "BoardModerators");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BoardModerators",
                table: "BoardModerators");

            migrationBuilder.DropIndex(
                name: "IX_BoardModerators_BoardId_Username",
                table: "BoardModerators");

            migrationBuilder.DropColumn(
                name: "CanBanUsers",
                table: "BoardModerators");

            migrationBuilder.DropColumn(
                name: "CanDeletePosts",
                table: "BoardModerators");

            migrationBuilder.DropColumn(
                name: "CanManageThreads",
                table: "BoardModerators");

            migrationBuilder.RenameTable(
                name: "BoardModerators",
                newName: "StaffAccount");

            migrationBuilder.AlterColumn<int>(
                name: "BoardId",
                table: "StaffAccount",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<bool>(
                name: "CanManageBoards",
                table: "StaffAccount",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CanManageModerators",
                table: "StaffAccount",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "StaffAccount",
                type: "character varying(21)",
                maxLength: 21,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "Permissions_BanUsers",
                table: "StaffAccount",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Permissions_DeletePosts",
                table: "StaffAccount",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Permissions_ManageThreads",
                table: "StaffAccount",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_StaffAccount",
                table: "StaffAccount",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_StaffAccount_BoardId",
                table: "StaffAccount",
                column: "BoardId");

            migrationBuilder.AddForeignKey(
                name: "FK_StaffAccount_Boards_BoardId",
                table: "StaffAccount",
                column: "BoardId",
                principalTable: "Boards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StaffAccount_Boards_BoardId",
                table: "StaffAccount");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StaffAccount",
                table: "StaffAccount");

            migrationBuilder.DropIndex(
                name: "IX_StaffAccount_BoardId",
                table: "StaffAccount");

            migrationBuilder.DropColumn(
                name: "CanManageBoards",
                table: "StaffAccount");

            migrationBuilder.DropColumn(
                name: "CanManageModerators",
                table: "StaffAccount");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "StaffAccount");

            migrationBuilder.DropColumn(
                name: "Permissions_BanUsers",
                table: "StaffAccount");

            migrationBuilder.DropColumn(
                name: "Permissions_DeletePosts",
                table: "StaffAccount");

            migrationBuilder.DropColumn(
                name: "Permissions_ManageThreads",
                table: "StaffAccount");

            migrationBuilder.RenameTable(
                name: "StaffAccount",
                newName: "BoardModerators");

            migrationBuilder.AlterColumn<int>(
                name: "BoardId",
                table: "BoardModerators",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CanBanUsers",
                table: "BoardModerators",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanDeletePosts",
                table: "BoardModerators",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanManageThreads",
                table: "BoardModerators",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_BoardModerators",
                table: "BoardModerators",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_BoardModerators_BoardId_Username",
                table: "BoardModerators",
                columns: new[] { "BoardId", "Username" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BoardModerators_Boards_BoardId",
                table: "BoardModerators",
                column: "BoardId",
                principalTable: "Boards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
