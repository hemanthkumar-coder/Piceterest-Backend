using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Picterest.Migrations
{
    /// <inheritdoc />
    public partial class AddedUpdatedAtColumntoUsersEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "Users");
        }
    }
}
