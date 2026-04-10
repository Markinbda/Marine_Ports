using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarinePorts.API.Migrations
{
    /// <inheritdoc />
    public partial class ExpandBoatFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BeamFeet",
                table: "Boats",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BeamInches",
                table: "Boats",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ColorBootLine",
                table: "Boats",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ColorBottom",
                table: "Boats",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ColorCabin",
                table: "Boats",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ColorDecks",
                table: "Boats",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ColorHull",
                table: "Boats",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DraughtFeet",
                table: "Boats",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DraughtInches",
                table: "Boats",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EngineMake",
                table: "Boats",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EngineSerialVin",
                table: "Boats",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EngineType",
                table: "Boats",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Fuel",
                table: "Boats",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HullNumber",
                table: "Boats",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LengthInches",
                table: "Boats",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Make",
                table: "Boats",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Material",
                table: "Boats",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PowerHp",
                table: "Boats",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhereBuilt",
                table: "Boats",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "YearBuilt",
                table: "Boats",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BeamFeet",
                table: "Boats");

            migrationBuilder.DropColumn(
                name: "BeamInches",
                table: "Boats");

            migrationBuilder.DropColumn(
                name: "ColorBootLine",
                table: "Boats");

            migrationBuilder.DropColumn(
                name: "ColorBottom",
                table: "Boats");

            migrationBuilder.DropColumn(
                name: "ColorCabin",
                table: "Boats");

            migrationBuilder.DropColumn(
                name: "ColorDecks",
                table: "Boats");

            migrationBuilder.DropColumn(
                name: "ColorHull",
                table: "Boats");

            migrationBuilder.DropColumn(
                name: "DraughtFeet",
                table: "Boats");

            migrationBuilder.DropColumn(
                name: "DraughtInches",
                table: "Boats");

            migrationBuilder.DropColumn(
                name: "EngineMake",
                table: "Boats");

            migrationBuilder.DropColumn(
                name: "EngineSerialVin",
                table: "Boats");

            migrationBuilder.DropColumn(
                name: "EngineType",
                table: "Boats");

            migrationBuilder.DropColumn(
                name: "Fuel",
                table: "Boats");

            migrationBuilder.DropColumn(
                name: "HullNumber",
                table: "Boats");

            migrationBuilder.DropColumn(
                name: "LengthInches",
                table: "Boats");

            migrationBuilder.DropColumn(
                name: "Make",
                table: "Boats");

            migrationBuilder.DropColumn(
                name: "Material",
                table: "Boats");

            migrationBuilder.DropColumn(
                name: "PowerHp",
                table: "Boats");

            migrationBuilder.DropColumn(
                name: "WhereBuilt",
                table: "Boats");

            migrationBuilder.DropColumn(
                name: "YearBuilt",
                table: "Boats");
        }
    }
}
