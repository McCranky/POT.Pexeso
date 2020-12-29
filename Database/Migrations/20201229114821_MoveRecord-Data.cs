using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class MoveRecordData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Data",
                table: "MoveRecord",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Data",
                table: "MoveRecord");
        }
    }
}
