using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddedPluginDataColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConsumerData",
                table: "Leases",
                type: "json",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "PublisherData",
                table: "Leases",
                type: "json",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsumerData",
                table: "Leases");

            migrationBuilder.DropColumn(
                name: "PublisherData",
                table: "Leases");
        }
    }
}
