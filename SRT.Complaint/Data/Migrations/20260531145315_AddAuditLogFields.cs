using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRT.Complaint.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Outcome",
                schema: "dbo",
                table: "AuditLogs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserAgent",
                schema: "dbo",
                table: "AuditLogs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Outcome",
                schema: "dbo",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "UserAgent",
                schema: "dbo",
                table: "AuditLogs");
        }
    }
}
