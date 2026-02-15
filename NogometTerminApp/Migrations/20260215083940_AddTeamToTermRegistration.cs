using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NogometTerminApp.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamToTermRegistration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Team",
                table: "TermRegistrations",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Team",
                table: "TermRegistrations");
        }
    }
}
