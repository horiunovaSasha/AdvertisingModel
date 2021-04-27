using Microsoft.EntityFrameworkCore.Migrations;

namespace AdvertisingModel.Data.Migrations
{
    public partial class extenduseridentity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "A",
                table: "AspNetUsers",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "C",
                table: "AspNetUsers",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Func",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "K0",
                table: "AspNetUsers",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "K1",
                table: "AspNetUsers",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "P",
                table: "AspNetUsers",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "R",
                table: "AspNetUsers",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "A",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "C",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Func",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "K0",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "K1",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "P",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "R",
                table: "AspNetUsers");
        }
    }
}
