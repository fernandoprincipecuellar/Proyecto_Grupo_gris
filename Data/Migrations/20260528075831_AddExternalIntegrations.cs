using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Proyecto_Grupo_gris.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalIntegrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EndCoordinates",
                table: "EcoRoutes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MapUrl",
                table: "EcoRoutes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StartCoordinates",
                table: "EcoRoutes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WeatherCondition",
                table: "EcoRoutes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "WeatherHumidity",
                table: "EcoRoutes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "WeatherTemperatureC",
                table: "EcoRoutes",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "WeatherWindSpeed",
                table: "EcoRoutes",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndCoordinates",
                table: "EcoRoutes");

            migrationBuilder.DropColumn(
                name: "MapUrl",
                table: "EcoRoutes");

            migrationBuilder.DropColumn(
                name: "StartCoordinates",
                table: "EcoRoutes");

            migrationBuilder.DropColumn(
                name: "WeatherCondition",
                table: "EcoRoutes");

            migrationBuilder.DropColumn(
                name: "WeatherHumidity",
                table: "EcoRoutes");

            migrationBuilder.DropColumn(
                name: "WeatherTemperatureC",
                table: "EcoRoutes");

            migrationBuilder.DropColumn(
                name: "WeatherWindSpeed",
                table: "EcoRoutes");
        }
    }
}
