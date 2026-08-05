using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Picterest.Migrations
{
    /// <inheritdoc />
    public partial class addedUserEntityWithGithubSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "varchar(100)", nullable: false),
                    email = table.Column<string>(type: "varchar(100)", nullable: false),
                    avatar_url = table.Column<string>(type: "varchar(500)", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    github_id = table.Column<long>(type: "bigint", nullable: false),
                    github_user_name = table.Column<string>(type: "varchar(100)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_github_id",
                table: "Users",
                column: "github_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_github_user_name",
                table: "Users",
                column: "github_user_name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
