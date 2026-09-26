using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnergyTrade.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIdempotencyRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IDEMPOTENCY_RECORDS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    KEY = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    OPERATION = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    RESPONSE = table.Column<string>(type: "CLOB", nullable: false),
                    STATUS_CODE = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CREATED_AT = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IDEMPOTENCY_RECORDS", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IDEMPOTENCY_RECORDS_KEY_OPERATION",
                table: "IDEMPOTENCY_RECORDS",
                columns: new[] { "KEY", "OPERATION" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IDEMPOTENCY_RECORDS");
        }
    }
}
