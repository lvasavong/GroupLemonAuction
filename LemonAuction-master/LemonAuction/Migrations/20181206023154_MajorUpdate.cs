using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LemonAuction.Migrations
{
    public partial class MajorUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentPrice",
                table: "Products");

            migrationBuilder.AddColumn<byte[]>(
                name: "Image",
                table: "Products",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Image",
                table: "Products");

            migrationBuilder.AddColumn<decimal>(
                name: "CurrentPrice",
                table: "Products",
                type: "decimal(11,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
