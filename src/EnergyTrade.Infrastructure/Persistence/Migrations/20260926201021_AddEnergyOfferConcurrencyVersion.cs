using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnergyTrade.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEnergyOfferConcurrencyVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VERSION",
                table: "ENERGY_OFFERS",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VERSION",
                table: "ENERGY_OFFERS");
        }
    }
}
