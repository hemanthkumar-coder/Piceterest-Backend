using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Picterest.Migrations
{
    /// <inheritdoc />
    public partial class AddStorageDeletionRelatedColumnsForFileTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "clean_up_status",
                table: "Files",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "delete_attempts",
                table: "Files",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "last_delete_error",
                table: "Files",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "storage_deleted_at",
                table: "Files",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "clean_up_status",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "delete_attempts",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "last_delete_error",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "storage_deleted_at",
                table: "Files");
        }
    }
}
