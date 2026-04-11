using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarinePorts.API.Migrations
{
    /// <inheritdoc />
    public partial class StripArcgisPrefix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Strip "ARCGIS-" prefix where the resulting number won't collide.
            migrationBuilder.Sql(@"
                UPDATE ""Moorings"" m
                SET ""MooringNumber"" = SUBSTRING(m.""MooringNumber"" FROM 8)
                WHERE m.""MooringNumber"" LIKE 'ARCGIS-%'
                  AND NOT EXISTS (
                      SELECT 1 FROM ""Moorings"" other
                      WHERE other.""MooringNumber"" = SUBSTRING(m.""MooringNumber"" FROM 8)
                        AND other.""Id"" <> m.""Id""
                  );
            ");

            // Delete any remaining ARCGIS- records – they are duplicates of records already in the table.
            migrationBuilder.Sql(@"
                DELETE FROM ""Moorings""
                WHERE ""MooringNumber"" LIKE 'ARCGIS-%';
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally not reversible – this is a one-way data clean-up.
        }
    }
}
