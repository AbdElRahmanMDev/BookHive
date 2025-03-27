using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookHive.Web.Data.Migrations
{
    public partial class RenameProperty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "categories",
                newName: "IsDeleted");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "categories",
                newName: "IsActive");
        }
    }
}
