using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrlShortner.Migrations
{
    /// <inheritdoc />
    public partial class AddVisitCountToShortenedUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "visit_count",
                table: "shortened_urls",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "visit_count",
                table: "shortened_urls");
        }
    }
}
