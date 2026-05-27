using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRT.Complaint.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeNoteAuthorNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComplaintNotes_StaffUsers_AuthorId",
                schema: "dbo",
                table: "ComplaintNotes");

            migrationBuilder.AlterColumn<int>(
                name: "AuthorId",
                schema: "dbo",
                table: "ComplaintNotes",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_ComplaintNotes_StaffUsers_AuthorId",
                schema: "dbo",
                table: "ComplaintNotes",
                column: "AuthorId",
                principalSchema: "dbo",
                principalTable: "StaffUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComplaintNotes_StaffUsers_AuthorId",
                schema: "dbo",
                table: "ComplaintNotes");

            migrationBuilder.AlterColumn<int>(
                name: "AuthorId",
                schema: "dbo",
                table: "ComplaintNotes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ComplaintNotes_StaffUsers_AuthorId",
                schema: "dbo",
                table: "ComplaintNotes",
                column: "AuthorId",
                principalSchema: "dbo",
                principalTable: "StaffUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
