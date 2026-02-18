using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AuditLogAdminFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLog_SystemAccount_UserId",
                table: "AuditLog");

            migrationBuilder.AlterColumn<short>(
                name: "UserId",
                table: "AuditLog",
                type: "smallint",
                nullable: true,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AddColumn<string>(
                name: "UserEmail",
                table: "AuditLog",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "AuditLog",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_Timestamp",
                table: "AuditLog",
                column: "Timestamp");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLog_SystemAccount_UserId",
                table: "AuditLog",
                column: "UserId",
                principalTable: "SystemAccount",
                principalColumn: "AccountID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLog_SystemAccount_UserId",
                table: "AuditLog");

            migrationBuilder.DropIndex(
                name: "IX_AuditLog_Timestamp",
                table: "AuditLog");

            migrationBuilder.DropColumn(
                name: "UserEmail",
                table: "AuditLog");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "AuditLog");

            migrationBuilder.AlterColumn<short>(
                name: "UserId",
                table: "AuditLog",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0,
                oldClrType: typeof(short),
                oldType: "smallint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLog_SystemAccount_UserId",
                table: "AuditLog",
                column: "UserId",
                principalTable: "SystemAccount",
                principalColumn: "AccountID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
