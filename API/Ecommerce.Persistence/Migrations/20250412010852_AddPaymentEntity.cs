using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ecommerce.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    PaymentId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IsSuccess = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VnpayTransactionId = table.Column<long>(type: "bigint", maxLength: 100, nullable: false),
                    PaymentMethod = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ResponseCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ResponseDescription = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    TransactionCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TransactionDescription = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    BankCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BankTransactionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.PaymentId);
                });

            migrationBuilder.CreateTable(
                name: "PaymentSalesOrder",
                columns: table => new
                {
                    PaymentsPaymentId = table.Column<long>(type: "bigint", nullable: false),
                    SalesOrderId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentSalesOrder", x => new { x.PaymentsPaymentId, x.SalesOrderId });
                    table.ForeignKey(
                        name: "FK_PaymentSalesOrder_Payments_PaymentsPaymentId",
                        column: x => x.PaymentsPaymentId,
                        principalTable: "Payments",
                        principalColumn: "PaymentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentSalesOrder_SalesOrders_SalesOrderId",
                        column: x => x.SalesOrderId,
                        principalTable: "SalesOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentSalesOrder_SalesOrderId",
                table: "PaymentSalesOrder",
                column: "SalesOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentSalesOrder");

            migrationBuilder.DropTable(
                name: "Payments");
        }
    }
}
