using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRT.Complaint.Data.Migrations
{
    /// <inheritdoc />
    public partial class SyncSystemSettingsSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SystemSettings",
                schema: "dbo",
                columns: table => new
                {
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Group = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedById = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Key);
                    table.ForeignKey(
                        name: "FK_SystemSettings_StaffUsers_UpdatedById",
                        column: x => x.UpdatedById,
                        principalSchema: "dbo",
                        principalTable: "StaffUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_Group",
                schema: "dbo",
                table: "SystemSettings",
                column: "Group");

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_UpdatedById",
                schema: "dbo",
                table: "SystemSettings",
                column: "UpdatedById");

            // Seed ค่าเริ่มต้น
            migrationBuilder.InsertData(
                schema: "dbo",
                table: "SystemSettings",
                columns: ["Key", "Value", "Group", "Label", "Description", "UpdatedAt", "UpdatedById"],
                values: new object[,]
                {
                    { "org.name",       "การรถไฟแห่งประเทศไทย",                                   "org", "ชื่อองค์กร (เต็ม)", "ใช้แสดงใน PDF รายงาน และอีเมล", new DateTime(2026,5,31,0,0,0,DateTimeKind.Utc), null },
                    { "org.name_short", "รฟท.",                                                   "org", "ชื่อย่อ",           null,                              new DateTime(2026,5,31,0,0,0,DateTimeKind.Utc), null },
                    { "org.address",    "1 ถ.รองเมือง แขวงรองเมือง เขตปทุมวัน กรุงเทพฯ 10330", "org", "ที่อยู่",           "แสดงใน footer และ PDF",           new DateTime(2026,5,31,0,0,0,DateTimeKind.Utc), null },
                    { "org.phone",      "1690",                                                   "org", "เบอร์โทรศัพท์",    "แสดงใน footer และหน้า FAQ",       new DateTime(2026,5,31,0,0,0,DateTimeKind.Utc), null },
                    { "org.email",      "complaint@railway.co.th",                               "org", "อีเมลติดต่อ",      null,                              new DateTime(2026,5,31,0,0,0,DateTimeKind.Utc), null },
                    { "org.website",    "https://www.railway.co.th",                             "org", "เว็บไซต์",         null,                              new DateTime(2026,5,31,0,0,0,DateTimeKind.Utc), null },
                }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemSettings",
                schema: "dbo");
        }
    }
}
