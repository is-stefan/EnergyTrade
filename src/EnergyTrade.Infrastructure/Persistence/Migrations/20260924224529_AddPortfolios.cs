using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnergyTrade.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPortfolios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "USER_ID",
                table: "POSITIONS",
                newName: "PORTFOLIO_ID");

            migrationBuilder.RenameIndex(
                name: "IX_POSITIONS_USER_ID_ENERGY_TYPE",
                table: "POSITIONS",
                newName: "IX_POSITIONS_PORTFOLIO_ID_ENERGY_TYPE");

            migrationBuilder.AddColumn<Guid>(
                name: "PORTFOLIO_ID",
                table: "ORDERS",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PORTFOLIO_ID",
                table: "ENERGY_OFFERS",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "PORTFOLIOS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    USER_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    NAME = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    BASE_CURRENCY = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    STATUS = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CREATED_AT = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    UPDATED_AT = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PORTFOLIOS", x => x.ID);
                });

            migrationBuilder.Sql(
                """
                INSERT INTO PORTFOLIOS
                (
                    ID,
                    USER_ID,
                    NAME,
                    BASE_CURRENCY,
                    STATUS,
                    CREATED_AT,
                    UPDATED_AT
                )
                SELECT
                    USER_ID,
                    USER_ID,
                    'Default Portfolio',
                    1,
                    1,
                    SYSTIMESTAMP,
                    NULL
                FROM
                (
                    SELECT BUYER_ID AS USER_ID FROM ORDERS
                    UNION
                    SELECT SELLER_ID AS USER_ID FROM ENERGY_OFFERS
                    UNION
                    SELECT PORTFOLIO_ID AS USER_ID FROM POSITIONS
                )
                """);

            migrationBuilder.Sql(
                """
                UPDATE ORDERS
                SET PORTFOLIO_ID = BUYER_ID
                """);

            migrationBuilder.Sql(
                """
                UPDATE ENERGY_OFFERS
                SET PORTFOLIO_ID = SELLER_ID
                """);

            migrationBuilder.CreateIndex(
                name: "IX_ORDERS_PORTFOLIO_ID",
                table: "ORDERS",
                column: "PORTFOLIO_ID");

            migrationBuilder.CreateIndex(
                name: "IX_ENERGY_OFFERS_PORTFOLIO_ID",
                table: "ENERGY_OFFERS",
                column: "PORTFOLIO_ID");

            migrationBuilder.CreateIndex(
                name: "IX_PORTFOLIOS_USER_ID",
                table: "PORTFOLIOS",
                column: "USER_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_ENERGY_OFFERS_PORTFOLIOS_PORTFOLIO_ID",
                table: "ENERGY_OFFERS",
                column: "PORTFOLIO_ID",
                principalTable: "PORTFOLIOS",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ORDERS_PORTFOLIOS_PORTFOLIO_ID",
                table: "ORDERS",
                column: "PORTFOLIO_ID",
                principalTable: "PORTFOLIOS",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_POSITIONS_PORTFOLIOS_PORTFOLIO_ID",
                table: "POSITIONS",
                column: "PORTFOLIO_ID",
                principalTable: "PORTFOLIOS",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ENERGY_OFFERS_PORTFOLIOS_PORTFOLIO_ID",
                table: "ENERGY_OFFERS");

            migrationBuilder.DropForeignKey(
                name: "FK_ORDERS_PORTFOLIOS_PORTFOLIO_ID",
                table: "ORDERS");

            migrationBuilder.DropForeignKey(
                name: "FK_POSITIONS_PORTFOLIOS_PORTFOLIO_ID",
                table: "POSITIONS");

            migrationBuilder.DropTable(
                name: "PORTFOLIOS");

            migrationBuilder.DropIndex(
                name: "IX_ORDERS_PORTFOLIO_ID",
                table: "ORDERS");

            migrationBuilder.DropIndex(
                name: "IX_ENERGY_OFFERS_PORTFOLIO_ID",
                table: "ENERGY_OFFERS");

            migrationBuilder.DropColumn(
                name: "PORTFOLIO_ID",
                table: "ORDERS");

            migrationBuilder.DropColumn(
                name: "PORTFOLIO_ID",
                table: "ENERGY_OFFERS");

            migrationBuilder.RenameColumn(
                name: "PORTFOLIO_ID",
                table: "POSITIONS",
                newName: "USER_ID");

            migrationBuilder.RenameIndex(
                name: "IX_POSITIONS_PORTFOLIO_ID_ENERGY_TYPE",
                table: "POSITIONS",
                newName: "IX_POSITIONS_USER_ID_ENERGY_TYPE");
        }
    }
}
