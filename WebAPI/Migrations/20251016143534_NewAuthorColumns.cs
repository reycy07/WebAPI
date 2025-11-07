using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class NewAuthorColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Authors",
                newName: "Names");

            migrationBuilder.AddColumn<string>(
                name: "Identification",
                table: "Authors",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastNames",
                table: "Authors",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Identification",
                table: "Authors");

            migrationBuilder.DropColumn(
                name: "LastNames",
                table: "Authors");

            migrationBuilder.RenameColumn(
                name: "Names",
                table: "Authors",
                newName: "Name");
        }
    }
}
