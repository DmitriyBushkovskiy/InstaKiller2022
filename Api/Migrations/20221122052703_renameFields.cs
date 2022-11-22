using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class renameFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Messages",
                newName: "State");

            migrationBuilder.RenameColumn(
                name: "SendingTime",
                table: "Messages",
                newName: "Created");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "State",
                table: "Messages",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "Created",
                table: "Messages",
                newName: "SendingTime");
        }
    }
}
