using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Picterest.Migrations
{
    /// <inheritdoc />
    public partial class AddedRestoredAtColumnToImageTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "restored_at",
                table: "Images",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "restored_at",
                table: "Images");
        }
    }
}
