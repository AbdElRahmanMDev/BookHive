using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookHive.Web.Data.Migrations
{
    public partial class add_new_proprty_ImageUrlThumb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrlThumb",
                table: "Books",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrlThumb",
                table: "Books");
        }
    }
}
