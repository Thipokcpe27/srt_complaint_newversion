using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SRT.Complaint.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "ComplaintCategories",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DepartmentName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DefaultPriority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplaintCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationTemplates",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LabelTh = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EmailSubject = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    EmailBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SmsBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsEmailEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsSmsEnabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StaffUsers",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeCode = table.Column<string>(type: "nchar(7)", fixedLength: true, maxLength: 7, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApiKeys",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    KeyType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    KeyPrefix = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    KeyHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    RateLimitPerMin = table.Column<int>(type: "int", nullable: false),
                    AllowedIps = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevokedById = table.Column<int>(type: "int", nullable: true),
                    RevokedReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiKeys_StaffUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalSchema: "dbo",
                        principalTable: "StaffUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ApiKeys_StaffUsers_RevokedById",
                        column: x => x.RevokedById,
                        principalSchema: "dbo",
                        principalTable: "StaffUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActorId = table.Column<int>(type: "int", nullable: true),
                    ActorCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EntityId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Detail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_StaffUsers_ActorId",
                        column: x => x.ActorId,
                        principalSchema: "dbo",
                        principalTable: "StaffUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Complaints",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ReporterName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ReporterPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ReporterEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    SubjectStation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
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
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SatisfactionScore = table.Column<byte>(type: "tinyint", nullable: true),
                    SatisfactionNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Complaints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Complaints_ComplaintCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "dbo",
                        principalTable: "ComplaintCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Complaints_StaffUsers_AssignedToId",
                        column: x => x.AssignedToId,
                        principalSchema: "dbo",
                        principalTable: "StaffUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SlaConfigs",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LabelTh = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ResolutionHours = table.Column<int>(type: "int", nullable: false),
                    AutoAssignAfterHours = table.Column<int>(type: "int", nullable: false),
                    WarningThresholdPercent = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedById = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlaConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SlaConfigs_StaffUsers_UpdatedById",
                        column: x => x.UpdatedById,
                        principalSchema: "dbo",
                        principalTable: "StaffUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ApiKeyScopes",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApiKeyId = table.Column<int>(type: "int", nullable: false),
                    Scope = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiKeyScopes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiKeyScopes_ApiKeys_ApiKeyId",
                        column: x => x.ApiKeyId,
                        principalSchema: "dbo",
                        principalTable: "ApiKeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApiRequestLogs",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApiKeyId = table.Column<int>(type: "int", nullable: false),
                    HttpMethod = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Endpoint = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    QueryString = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ResponseStatus = table.Column<int>(type: "int", nullable: false),
                    ResponseMs = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiRequestLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiRequestLogs_ApiKeys_ApiKeyId",
                        column: x => x.ApiKeyId,
                        principalSchema: "dbo",
                        principalTable: "ApiKeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Webhooks",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApiKeyId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TargetUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    SecretHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Events = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastTriggeredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastStatusCode = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Webhooks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Webhooks_ApiKeys_ApiKeyId",
                        column: x => x.ApiKeyId,
                        principalSchema: "dbo",
                        principalTable: "ApiKeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComplaintAttachments",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComplaintId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    StoredPath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    MimeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplaintAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplaintAttachments_Complaints_ComplaintId",
                        column: x => x.ComplaintId,
                        principalSchema: "dbo",
                        principalTable: "Complaints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComplaintNotes",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComplaintId = table.Column<int>(type: "int", nullable: false),
                    AuthorId = table.Column<int>(type: "int", nullable: false),
                    NoteType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplaintNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplaintNotes_Complaints_ComplaintId",
                        column: x => x.ComplaintId,
                        principalSchema: "dbo",
                        principalTable: "Complaints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComplaintNotes_StaffUsers_AuthorId",
                        column: x => x.AuthorId,
                        principalSchema: "dbo",
                        principalTable: "StaffUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComplaintTransferLog",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComplaintId = table.Column<int>(type: "int", nullable: false),
                    FromOfficerId = table.Column<int>(type: "int", nullable: true),
                    ToOfficerId = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TransferredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsAutoAssign = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplaintTransferLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplaintTransferLog_Complaints_ComplaintId",
                        column: x => x.ComplaintId,
                        principalSchema: "dbo",
                        principalTable: "Complaints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComplaintTransferLog_StaffUsers_FromOfficerId",
                        column: x => x.FromOfficerId,
                        principalSchema: "dbo",
                        principalTable: "StaffUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ComplaintTransferLog_StaffUsers_ToOfficerId",
                        column: x => x.ToOfficerId,
                        principalSchema: "dbo",
                        principalTable: "StaffUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WebhookDeliveryLogs",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WebhookId = table.Column<int>(type: "int", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AttemptCount = table.Column<byte>(type: "tinyint", nullable: false),
                    ResponseStatus = table.Column<int>(type: "int", nullable: true),
                    ResponseBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDelivered = table.Column<bool>(type: "bit", nullable: false),
                    NextRetryAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebhookDeliveryLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebhookDeliveryLogs_Webhooks_WebhookId",
                        column: x => x.WebhookId,
                        principalSchema: "dbo",
                        principalTable: "Webhooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "ComplaintCategories",
                columns: new[] { "Id", "DefaultPriority", "DepartmentName", "IsActive", "Name", "SortOrder" },
                values: new object[,]
                {
                    { 1, "Normal", "ฝ่ายการเดินรถ", true, "ความตรงต่อเวลา", 1 },
                    { 2, "Normal", "ฝ่ายการโดยสาร", true, "บริการบนขบวนรถ", 2 },
                    { 3, "Normal", "ฝ่ายบริหารทรัพยากรบุคคล", true, "พนักงาน / มารยาท", 3 },
                    { 4, "Normal", "ฝ่ายโยธา", true, "สิ่งอำนวยความสะดวก", 4 },
                    { 5, "Normal", "ฝ่ายบริการสถานี", true, "ความสะอาด", 5 },
                    { 6, "High", "ฝ่ายการพาณิชย์", true, "ตั๋ว / การคืนเงิน", 6 },
                    { 7, "Urgent", "ฝ่ายรักษาความปลอดภัย", true, "ความปลอดภัย", 7 },
                    { 8, "Normal", "ฝ่ายบริการสถานี", true, "สถานี / ที่จอดรถ", 8 },
                    { 9, "Normal", null, true, "อื่น ๆ", 9 }
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "NotificationTemplates",
                columns: new[] { "Id", "EmailBody", "EmailSubject", "EventKey", "IsEmailEnabled", "IsSmsEnabled", "LabelTh", "SmsBody" },
                values: new object[,]
                {
                    { 1, null, "[รฟท.] รับเรื่องร้องเรียนของท่านแล้ว เลขที่ {ReferenceNumber}", "ComplaintReceived", true, true, "รับเรื่องร้องเรียนแล้ว", "[รฟท.] รับเรื่องร้องเรียนของท่านแล้ว เลขที่ {ReferenceNumber} ติดตามสถานะ: {TrackingUrl}" },
                    { 2, null, "[รฟท.] อัปเดตสถานะเรื่องร้องเรียน {ReferenceNumber}", "StatusChanged", true, true, "สถานะเรื่องเปลี่ยนแปลง", "[รฟท.] เรื่อง {ReferenceNumber} อัปเดตสถานะเป็น: {Status}" },
                    { 3, null, "[รฟท.] เรื่องร้องเรียน {ReferenceNumber} ได้รับการแก้ไขแล้ว", "ComplaintClosed", true, true, "ปิดเรื่องร้องเรียนแล้ว", "[รฟท.] เรื่อง {ReferenceNumber} ปิดแล้ว ขอบคุณที่ใช้บริการ" },
                    { 4, null, "[รฟท.] มอบหมายเรื่องร้องเรียนให้ท่าน {ReferenceNumber}", "AutoAssigned", true, false, "มอบหมายเรื่องอัตโนมัติ", null },
                    { 5, null, "[รฟท.] เตือน: เรื่อง {ReferenceNumber} ใกล้ครบกำหนด SLA", "SlaWarning", true, false, "เตือน SLA ใกล้ครบกำหนด", null },
                    { 6, null, "[รฟท.] แจ้งเตือน: เรื่อง {ReferenceNumber} เกิน SLA แล้ว", "SlaBreached", true, false, "SLA เกินกำหนดแล้ว", null },
                    { 7, null, "[รฟท.] เร่งด่วน: รับเรื่องร้องเรียนระดับ Critical {ReferenceNumber}", "CriticalReceived", true, false, "รับเรื่องเร่งด่วนมาก", null },
                    { 8, null, "[รฟท.] แจ้งเตือน: มีการขอดูข้อมูลผู้แจ้ง {ReferenceNumber}", "DecryptionRequested", true, false, "ขอดูข้อมูลผู้แจ้งทุจริต", null }
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "SlaConfigs",
                columns: new[] { "Id", "AutoAssignAfterHours", "LabelTh", "Priority", "ResolutionHours", "UpdatedAt", "UpdatedById", "WarningThresholdPercent" },
                values: new object[,]
                {
                    { 1, 1, "เร่งด่วนมาก (ความปลอดภัย)", "Critical", 24, new DateTime(2025, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 80 },
                    { 2, 4, "เร่งด่วน", "Urgent", 72, new DateTime(2025, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 80 },
                    { 3, 8, "สำคัญ", "High", 120, new DateTime(2025, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 80 },
                    { 4, 12, "ปกติ", "Normal", 168, new DateTime(2025, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 80 },
                    { 5, 24, "ข้อเสนอแนะ", "Low", 360, new DateTime(2025, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 80 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_CreatedById",
                schema: "dbo",
                table: "ApiKeys",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_RevokedById",
                schema: "dbo",
                table: "ApiKeys",
                column: "RevokedById");

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeyScopes_ApiKeyId",
                schema: "dbo",
                table: "ApiKeyScopes",
                column: "ApiKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiRequestLogs_ApiKeyId_CreatedAt",
                schema: "dbo",
                table: "ApiRequestLogs",
                columns: new[] { "ApiKeyId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ActorId",
                schema: "dbo",
                table: "AuditLogs",
                column: "ActorId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintAttachments_ComplaintId",
                schema: "dbo",
                table: "ComplaintAttachments",
                column: "ComplaintId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintNotes_AuthorId",
                schema: "dbo",
                table: "ComplaintNotes",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintNotes_ComplaintId",
                schema: "dbo",
                table: "ComplaintNotes",
                column: "ComplaintId");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_AssignedToId",
                schema: "dbo",
                table: "Complaints",
                column: "AssignedToId");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_CategoryId",
                schema: "dbo",
                table: "Complaints",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_ReferenceNumber",
                schema: "dbo",
                table: "Complaints",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintTransferLog_ComplaintId",
                schema: "dbo",
                table: "ComplaintTransferLog",
                column: "ComplaintId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintTransferLog_FromOfficerId",
                schema: "dbo",
                table: "ComplaintTransferLog",
                column: "FromOfficerId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintTransferLog_ToOfficerId",
                schema: "dbo",
                table: "ComplaintTransferLog",
                column: "ToOfficerId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_EventKey",
                schema: "dbo",
                table: "NotificationTemplates",
                column: "EventKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SlaConfigs_Priority",
                schema: "dbo",
                table: "SlaConfigs",
                column: "Priority",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SlaConfigs_UpdatedById",
                schema: "dbo",
                table: "SlaConfigs",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_StaffUsers_EmployeeCode",
                schema: "dbo",
                table: "StaffUsers",
                column: "EmployeeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WebhookDeliveryLogs_WebhookId",
                schema: "dbo",
                table: "WebhookDeliveryLogs",
                column: "WebhookId");

            migrationBuilder.CreateIndex(
                name: "IX_Webhooks_ApiKeyId",
                schema: "dbo",
                table: "Webhooks",
                column: "ApiKeyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiKeyScopes",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ApiRequestLogs",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "AuditLogs",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ComplaintAttachments",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ComplaintNotes",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ComplaintTransferLog",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "NotificationTemplates",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "SlaConfigs",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "WebhookDeliveryLogs",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Complaints",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Webhooks",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ComplaintCategories",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ApiKeys",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "StaffUsers",
                schema: "dbo");
        }
    }
}
