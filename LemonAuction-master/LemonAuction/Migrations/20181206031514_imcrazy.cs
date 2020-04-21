using Microsoft.EntityFrameworkCore.Migrations;

namespace LemonAuction.Migrations
{
    public partial class imcrazy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Pleber",
                table: "Products",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Pleber",
                table: "Products");
        }
    }
}
