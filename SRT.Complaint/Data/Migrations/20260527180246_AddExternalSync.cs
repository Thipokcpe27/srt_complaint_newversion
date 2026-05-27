using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRT.Complaint.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                schema: "dbo",
                table: "Complaints",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalSystem",
                schema: "dbo",
                table: "Complaints",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ExternalSyncLogs",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExternalSystem = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FetchedCount = table.Column<int>(type: "int", nullable: false),
                    NewCount = table.Column<int>(type: "int", nullable: false),
                    DuplicateCount = table.Column<int>(type: "int", nullable: false),
                    ErrorCount = table.Column<int>(type: "int", nullable: false),
                    SyncStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TriggeredById = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalSyncLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExternalSyncLogs_StaffUsers_TriggeredById",
                        column: x => x.TriggeredById,
                        principalSchema: "dbo",
                        principalTable: "StaffUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_ExternalSystem_ExternalId",
                schema: "dbo",
                table: "Complaints",
                columns: new[] { "ExternalSystem", "ExternalId" },
                unique: true,
                filter: "[ExternalId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalSyncLogs_TriggeredById",
                schema: "dbo",
                table: "ExternalSyncLogs",
                column: "TriggeredById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExternalSyncLogs",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_Complaints_ExternalSystem_ExternalId",
                schema: "dbo",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                schema: "dbo",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "ExternalSystem",
                schema: "dbo",
                table: "Complaints");
        }
    }
}
