using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SRT.Complaint.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedSubCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [dbo].[ComplaintSubCategories]");
            migrationBuilder.Sql("DBCC CHECKIDENT ('[dbo].[ComplaintSubCategories]', RESEED, 0)");

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "ComplaintSubCategories",
                columns: new[] { "Id", "CategoryId", "IsActive", "Name", "SortOrder" },
                values: new object[,]
                {
                    { 1, 1, true, "รถออกล่าช้า", 1 },
                    { 2, 1, true, "รถมาถึงล่าช้า", 2 },
                    { 3, 1, true, "ยกเลิกขบวนโดยไม่แจ้งล่วงหน้า", 3 },
                    { 4, 1, true, "หยุดกลางทางนานผิดปกติ", 4 },
                    { 5, 1, true, "เวลาจอดสถานีไม่ตรงตารางเวลา", 5 },
                    { 6, 2, true, "ที่นั่ง / เตียงชำรุด", 1 },
                    { 7, 2, true, "แอร์ / พัดลมไม่ทำงาน", 2 },
                    { 8, 2, true, "ห้องน้ำบนขบวนชำรุด / สกปรก", 3 },
                    { 9, 2, true, "บริการอาหารและเครื่องดื่ม", 4 },
                    { 10, 2, true, "เต้ารับไฟฟ้าชำรุด", 5 },
                    { 11, 2, true, "Wi-Fi / ระบบบันเทิงขัดข้อง", 6 },
                    { 12, 3, true, "พูดจาไม่สุภาพ / ไม่ให้เกียรติ", 1 },
                    { 13, 3, true, "บริการล่าช้า / ไม่ใส่ใจ", 2 },
                    { 14, 3, true, "ให้ข้อมูลผิด / ไม่ถูกต้อง", 3 },
                    { 15, 3, true, "พฤติกรรมไม่เหมาะสม", 4 },
                    { 16, 3, true, "เลือกปฏิบัติ", 5 },
                    { 17, 4, true, "ลิฟต์ / บันไดเลื่อนขัดข้อง", 1 },
                    { 18, 4, true, "ที่นั่งรอในสถานีชำรุด", 2 },
                    { 19, 4, true, "สิ่งอำนวยความสะดวกสำหรับผู้พิการ", 3 },
                    { 20, 4, true, "ป้ายบอกทาง / ข้อมูลไม่ชัดเจน", 4 },
                    { 21, 4, true, "ระบบประกาศเสียงขัดข้อง", 5 },
                    { 22, 5, true, "ความสะอาดในขบวนรถ", 1 },
                    { 23, 5, true, "ความสะอาดในสถานี", 2 },
                    { 24, 5, true, "ห้องน้ำสกปรก", 3 },
                    { 25, 5, true, "การกำจัดขยะ", 4 },
                    { 26, 6, true, "ปัญหาการจองตั๋ว", 1 },
                    { 27, 6, true, "ขอคืนเงินค่าตั๋ว", 2 },
                    { 28, 6, true, "ราคาตั๋วไม่ถูกต้อง", 3 },
                    { 29, 6, true, "ระบบออนไลน์ขัดข้อง", 4 },
                    { 30, 6, true, "ปัญหาโปรโมชั่น / ส่วนลด", 5 },
                    { 31, 7, true, "การโจรกรรม / ทรัพย์สินสูญหาย", 1 },
                    { 32, 7, true, "การล่วงละเมิด / คุกคาม", 2 },
                    { 33, 7, true, "ผู้โดยสารก่อกวน / พฤติกรรมน่ารังเกียจ", 3 },
                    { 34, 7, true, "อุบัติเหตุในบริเวณสถานี", 4 },
                    { 35, 7, true, "การลักลอบนำสิ่งของอันตราย", 5 },
                    { 36, 8, true, "ที่จอดรถไม่เพียงพอ", 1 },
                    { 37, 8, true, "ค่าจอดรถไม่ถูกต้อง", 2 },
                    { 38, 8, true, "สิ่งก่อสร้างกีดขวาง / ทางเข้าออก", 3 },
                    { 39, 8, true, "ไฟส่องสว่างไม่เพียงพอ", 4 },
                    { 40, 9, true, "ข้อเสนอแนะทั่วไป", 1 },
                    { 41, 9, true, "ไม่อยู่ในหมวดหมู่ข้างต้น", 2 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "ComplaintSubCategories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "ComplaintSubCategories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "ComplaintSubCategories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "ComplaintSubCategories",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "ComplaintSubCategories",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "ComplaintSubCategories",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "ComplaintSubCategories",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "ComplaintSubCategories",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "ComplaintSubCategories",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "ComplaintSubCategories",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "ComplaintSubCategories",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "ComplaintSubCategories",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "ComplaintSubCategories",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "ComplaintSubCategories",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "ComplaintSubCategories",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "ComplaintSubCategories",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "ComplaintSubCategories",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "ComplaintSubCategories",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "ComplaintSubCategories",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "ComplaintSubCategories",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "ComplaintSubCategories",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "ComplaintSubCategories",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "ComplaintSubCategories",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "ComplaintSubCategories",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "ComplaintSubCategories",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "ComplaintSubCategories",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "ComplaintSubCategories",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "ComplaintSubCategories",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "ComplaintSubCategories",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "ComplaintSubCategories",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "ComplaintSubCategories",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "ComplaintSubCategories",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "ComplaintSubCategories",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "ComplaintSubCategories",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "ComplaintSubCategories",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "ComplaintSubCategories",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "ComplaintSubCategories",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "ComplaintSubCategories",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "ComplaintSubCategories",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "ComplaintSubCategories",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "ComplaintSubCategories",
                keyColumn: "Id",
                keyValue: 41);
        }
    }
}
