using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnergyTrade.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIdempotencyRequestHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM "IDEMPOTENCY_RECORDS"
                """);

            migrationBuilder.AddColumn<string>(
                name: "REQUEST_HASH",
                table: "IDEMPOTENCY_RECORDS",
                type: "NVARCHAR2(64)",
                maxLength: 64,
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "REQUEST_HASH",
                table: "IDEMPOTENCY_RECORDS");
        }
    }
}
