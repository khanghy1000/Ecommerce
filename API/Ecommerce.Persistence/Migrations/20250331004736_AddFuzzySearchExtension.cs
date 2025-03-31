using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFuzzySearchExtension : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder
                .AlterDatabase()
                .Annotation("Npgsql:Enum:product_status", "active,deleted,inactive")
                .Annotation("Npgsql:PostgresExtension:fuzzystrmatch", ",,")
                .OldAnnotation("Npgsql:Enum:product_status", "active,deleted,inactive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder
                .AlterDatabase()
                .Annotation("Npgsql:Enum:product_status", "active,deleted,inactive")
                .OldAnnotation("Npgsql:Enum:product_status", "active,deleted,inactive")
                .OldAnnotation("Npgsql:PostgresExtension:fuzzystrmatch", ",,");
        }
    }
}
