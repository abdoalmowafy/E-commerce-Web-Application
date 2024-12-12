using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Egost.Migrations
{
    /// <inheritdoc />
    public partial class AddtypeandIdtoeditshistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Field",
                table: "EditHistories",
                newName: "EditedType");

            migrationBuilder.RenameColumn(
                name: "DeletedModelName",
                table: "DeletesHistory",
                newName: "DeletedType");

            migrationBuilder.AddColumn<string>(
                name: "EditedField",
                table: "EditHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EditedId",
                table: "EditHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EditedField",
                table: "EditHistories");

            migrationBuilder.DropColumn(
                name: "EditedId",
                table: "EditHistories");

            migrationBuilder.RenameColumn(
                name: "EditedType",
                table: "EditHistories",
                newName: "Field");

            migrationBuilder.RenameColumn(
                name: "DeletedType",
                table: "DeletesHistory",
                newName: "DeletedModelName");
        }
    }
}
