using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRT.Complaint.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSubCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SubCategoryId",
                schema: "dbo",
                table: "Complaints",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ComplaintSubCategories",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplaintSubCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplaintSubCategories_ComplaintCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "dbo",
                        principalTable: "ComplaintCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_SubCategoryId",
                schema: "dbo",
                table: "Complaints",
                column: "SubCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintSubCategories_CategoryId",
                schema: "dbo",
                table: "ComplaintSubCategories",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_ComplaintSubCategories_SubCategoryId",
                schema: "dbo",
                table: "Complaints",
                column: "SubCategoryId",
                principalSchema: "dbo",
                principalTable: "ComplaintSubCategories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_ComplaintSubCategories_SubCategoryId",
                schema: "dbo",
                table: "Complaints");

            migrationBuilder.DropTable(
                name: "ComplaintSubCategories",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_Complaints_SubCategoryId",
                schema: "dbo",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "SubCategoryId",
                schema: "dbo",
                table: "Complaints");
        }
    }
}
