using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRT.Complaint.Data.Migrations.Corruption
{
    /// <inheritdoc />
    public partial class AddCorruptionPiiDeletedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PiiDeletedAt",
                schema: "corruption",
                table: "Reports",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PiiDeletedAt",
                schema: "corruption",
                table: "Reports");
        }
    }
}
