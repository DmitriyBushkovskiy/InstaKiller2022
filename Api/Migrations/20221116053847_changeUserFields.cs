using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class changeUserFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "isActive",
                table: "Users",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "IsPhineConfirmed",
                table: "Users",
                newName: "IsPhoneConfirmed");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Users",
                newName: "isActive");

            migrationBuilder.RenameColumn(
                name: "IsPhoneConfirmed",
                table: "Users",
                newName: "IsPhineConfirmed");
        }
    }
}
