using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarinePorts.API.Migrations
{
    /// <inheritdoc />
    public partial class AddMooringIdColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MooringId",
                table: "Boats",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Boats_MooringId",
                table: "Boats",
                column: "MooringId");

            migrationBuilder.AddForeignKey(
                name: "FK_Boats_Moorings_MooringId",
                table: "Boats",
                column: "MooringId",
                principalTable: "Moorings",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Boats_Moorings_MooringId",
                table: "Boats");

            migrationBuilder.DropIndex(
                name: "IX_Boats_MooringId",
                table: "Boats");

            migrationBuilder.DropColumn(
                name: "MooringId",
                table: "Boats");
        }
    }
}
