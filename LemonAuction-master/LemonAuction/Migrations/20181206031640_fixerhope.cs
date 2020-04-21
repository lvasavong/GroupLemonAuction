using Microsoft.EntityFrameworkCore.Migrations;

namespace LemonAuction.Migrations
{
    public partial class fixerhope : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Pleber",
                table: "Products");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Pleber",
                table: "Products",
                nullable: true);
        }
    }
}
