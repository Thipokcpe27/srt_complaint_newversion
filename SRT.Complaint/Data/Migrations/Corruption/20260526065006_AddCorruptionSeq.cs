using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRT.Complaint.Data.Migrations.Corruption
{
    /// <inheritdoc />
    public partial class AddCorruptionSeq : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE SEQUENCE corruption.CorruptionSeq START WITH 1 INCREMENT BY 1 NO CYCLE");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP SEQUENCE IF EXISTS corruption.CorruptionSeq");
        }
    }
}
