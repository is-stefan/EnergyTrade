using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnergyTrade.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTrades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TRADES",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ENERGY_OFFER_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    SELLER_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    BUYER_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ENERGY_TYPE = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    QUANTITY_MWH = table.Column<decimal>(type: "DECIMAL(18,3)", precision: 18, scale: 3, nullable: false),
                    PRICE_PER_MWH = table.Column<decimal>(type: "DECIMAL(18,2)", precision: 18, scale: 2, nullable: false),
                    CURRENCY = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CREATED_AT = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRADES", x => x.ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TRADES");
        }
    }
}
