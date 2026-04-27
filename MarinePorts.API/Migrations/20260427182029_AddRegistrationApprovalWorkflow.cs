using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarinePorts.API.Migrations
{
    /// <inheritdoc />
    public partial class AddRegistrationApprovalWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "Moorings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Moorings",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RenewalRequestedAt",
                table: "Moorings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "Boats",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Boats",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RenewalRequestedAt",
                table: "Boats",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.Sql("UPDATE \"Boats\" SET \"ExpiresAt\" = \"RegisteredAt\" + INTERVAL '1 year', \"IsApproved\" = TRUE WHERE \"ExpiresAt\" IS NULL;");
            migrationBuilder.Sql("UPDATE \"Moorings\" SET \"ExpiresAt\" = \"RegisteredAt\" + INTERVAL '1 year', \"IsApproved\" = TRUE WHERE \"ExpiresAt\" IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "Moorings");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Moorings");

            migrationBuilder.DropColumn(
                name: "RenewalRequestedAt",
                table: "Moorings");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "Boats");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Boats");

            migrationBuilder.DropColumn(
                name: "RenewalRequestedAt",
                table: "Boats");
        }
    }
}
