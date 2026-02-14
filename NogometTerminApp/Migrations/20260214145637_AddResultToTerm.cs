using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NogometTerminApp.Migrations
{
    /// <inheritdoc />
    public partial class AddResultToTerm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Result",
                table: "Terms",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Result",
                table: "Terms");
        }
    }
}
