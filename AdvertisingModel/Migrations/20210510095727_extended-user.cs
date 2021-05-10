using Microsoft.EntityFrameworkCore.Migrations;

namespace AdvertisingModel.Migrations
{
    public partial class extendeduser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "K2",
                table: "AspNetUsers",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "K3",
                table: "AspNetUsers",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "K2",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "K3",
                table: "AspNetUsers");
        }
    }
}
