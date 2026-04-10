using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarinePorts.API.Migrations
{
    /// <inheritdoc />
    public partial class SplitFullNameToFirstLast : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            // Migrate existing FullName data
            migrationBuilder.Sql("""
                UPDATE "Users"
                SET "FirstName" = SPLIT_PART("FullName", ' ', 1),
                    "LastName"  = CASE
                        WHEN POSITION(' ' IN "FullName") > 0
                        THEN SUBSTRING("FullName" FROM POSITION(' ' IN "FullName") + 1)
                        ELSE ''
                    END
                """);

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE "Users" SET "FullName" = "FirstName" || ' ' || "LastName"
                """);

            migrationBuilder.DropColumn(name: "FirstName", table: "Users");
            migrationBuilder.DropColumn(name: "LastName",  table: "Users");
        }
    }
}
