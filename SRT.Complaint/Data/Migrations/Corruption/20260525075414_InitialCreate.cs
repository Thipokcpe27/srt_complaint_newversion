using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRT.Complaint.Data.Migrations.Corruption
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "corruption");

            migrationBuilder.CreateTable(
                name: "Reports",
                schema: "corruption",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ReporterNameEncrypted = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    ReporterPhoneEncrypted = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    ReporterEmailEncrypted = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    ReporterIdCardEncrypted = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    ReporterNameMasked = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ReporterPhoneMasked = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ReporterEmailMasked = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SubjectType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SubjectPersonName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SubjectDepartment = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IncidentDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AssignedToId = table.Column<int>(type: "int", nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SlaDeadline = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SlaBreached = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DecryptionLogs",
                schema: "corruption",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportId = table.Column<int>(type: "int", nullable: false),
                    RequestedById = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DecryptionLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DecryptionLogs_Reports_ReportId",
                        column: x => x.ReportId,
                        principalSchema: "corruption",
                        principalTable: "Reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvestigationLogs",
                schema: "corruption",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportId = table.Column<int>(type: "int", nullable: false),
                    AuthorId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsConfidential = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestigationLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvestigationLogs_Reports_ReportId",
                        column: x => x.ReportId,
                        principalSchema: "corruption",
                        principalTable: "Reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DecryptionLogs_ReportId",
                schema: "corruption",
                table: "DecryptionLogs",
                column: "ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestigationLogs_ReportId",
                schema: "corruption",
                table: "InvestigationLogs",
                column: "ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ReferenceNumber",
                schema: "corruption",
                table: "Reports",
                column: "ReferenceNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DecryptionLogs",
                schema: "corruption");

            migrationBuilder.DropTable(
                name: "InvestigationLogs",
                schema: "corruption");

            migrationBuilder.DropTable(
                name: "Reports",
                schema: "corruption");
        }
    }
}
