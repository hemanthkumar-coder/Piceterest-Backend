using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Picterest.Migrations
{
    /// <inheritdoc />
    public partial class createFileTableAndChangesRequiredInImageTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "img_base64",
                table: "Images");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "Images",
                type: "varchar(100)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "Images",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FileId",
                table: "Images",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_name = table.Column<string>(type: "varchar(100)", nullable: false),
                    object_key = table.Column<string>(type: "varchar(500)", nullable: false),
                    bucket = table.Column<string>(type: "varchar(100)", nullable: false),
                    content_type = table.Column<string>(type: "varchar(100)", nullable: false),
                    size = table.Column<long>(type: "bigint", nullable: false),
                    uploaded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Images_FileId",
                table: "Images",
                column: "FileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Files_bucket",
                table: "Files",
                column: "bucket");

            migrationBuilder.CreateIndex(
                name: "IX_Files_object_key",
                table: "Files",
                column: "object_key",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Images_Files_FileId",
                table: "Images",
                column: "FileId",
                principalTable: "Files",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Images_Files_FileId",
                table: "Images");

            migrationBuilder.DropTable(
                name: "Files");

            migrationBuilder.DropIndex(
                name: "IX_Images_FileId",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "FileId",
                table: "Images");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "Images",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "Images",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "img_base64",
                table: "Images",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
