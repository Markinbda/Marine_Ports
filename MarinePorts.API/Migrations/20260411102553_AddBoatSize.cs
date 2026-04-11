using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarinePorts.API.Migrations
{
    /// <inheritdoc />
    public partial class AddBoatSize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Colour",
                table: "Moorings");

            migrationBuilder.RenameColumn(
                name: "Source",
                table: "Moorings",
                newName: "BoatSize");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BoatSize",
                table: "Moorings",
                newName: "Source");

            migrationBuilder.AddColumn<string>(
                name: "Colour",
                table: "Moorings",
                type: "text",
                nullable: true);
        }
    }
}
