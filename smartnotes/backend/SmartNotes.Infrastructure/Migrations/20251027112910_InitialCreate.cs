using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartNotes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "smartnotes");

            migrationBuilder.CreateTable(
                name: "Notes",
                schema: "smartnotes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    content = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false),
                    tags = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notes", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_notes_created_at",
                schema: "smartnotes",
                table: "Notes",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_notes_tags_gin",
                schema: "smartnotes",
                table: "Notes",
                column: "tags")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "ix_notes_title",
                schema: "smartnotes",
                table: "Notes",
                column: "title");

            migrationBuilder.CreateIndex(
                name: "ix_notes_updated_at",
                schema: "smartnotes",
                table: "Notes",
                column: "updated_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notes",
                schema: "smartnotes");
        }
    }
}
