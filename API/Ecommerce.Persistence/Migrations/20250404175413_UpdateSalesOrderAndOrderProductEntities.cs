using Ecommerce.Domain;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSalesOrderAndOrderProductEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder
                .AlterDatabase()
                .Annotation("Npgsql:Enum:payment_method", "cod,vnpay")
                .Annotation(
                    "Npgsql:Enum:sales_order_status",
                    "cancelled,delivered,pending_confirmation,pending_payment,tracking"
                )
                .Annotation("Npgsql:PostgresExtension:fuzzystrmatch", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:fuzzystrmatch", ",,");

            migrationBuilder.AddColumn<PaymentMethod>(
                name: "PaymentMethod",
                table: "SalesOrders",
                type: "payment_method",
                nullable: false,
                defaultValue: PaymentMethod.Cod
            );

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress",
                table: "SalesOrders",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<int>(
                name: "ShippingFee",
                table: "SalesOrders",
                type: "integer",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddColumn<string>(
                name: "ShippingName",
                table: "SalesOrders",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<string>(
                name: "ShippingOrderCode",
                table: "SalesOrders",
                type: "character varying(36)",
                maxLength: 36,
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "ShippingPhone",
                table: "SalesOrders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<int>(
                name: "ShippingWardId",
                table: "SalesOrders",
                type: "integer",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddColumn<SalesOrderStatus>(
                name: "Status",
                table: "SalesOrders",
                type: "sales_order_status",
                nullable: false,
                defaultValue: SalesOrderStatus.PendingPayment
            );

            migrationBuilder.AddColumn<decimal>(
                name: "Subtotal",
                table: "SalesOrders",
                type: "numeric",
                nullable: false,
                defaultValue: 0m
            );

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "OrderProducts",
                type: "integer",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_ShippingWardId",
                table: "SalesOrders",
                column: "ShippingWardId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_OrderProducts_ProductId",
                table: "OrderProducts",
                column: "ProductId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_OrderProducts_Products_ProductId",
                table: "OrderProducts",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrders_Wards_ShippingWardId",
                table: "SalesOrders",
                column: "ShippingWardId",
                principalTable: "Wards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderProducts_Products_ProductId",
                table: "OrderProducts"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrders_Wards_ShippingWardId",
                table: "SalesOrders"
            );

            migrationBuilder.DropIndex(name: "IX_SalesOrders_ShippingWardId", table: "SalesOrders");

            migrationBuilder.DropIndex(name: "IX_OrderProducts_ProductId", table: "OrderProducts");

            migrationBuilder.DropColumn(name: "PaymentMethod", table: "SalesOrders");

            migrationBuilder.DropColumn(name: "ShippingAddress", table: "SalesOrders");

            migrationBuilder.DropColumn(name: "ShippingFee", table: "SalesOrders");

            migrationBuilder.DropColumn(name: "ShippingName", table: "SalesOrders");

            migrationBuilder.DropColumn(name: "ShippingOrderCode", table: "SalesOrders");

            migrationBuilder.DropColumn(name: "ShippingPhone", table: "SalesOrders");

            migrationBuilder.DropColumn(name: "ShippingWardId", table: "SalesOrders");

            migrationBuilder.DropColumn(name: "Status", table: "SalesOrders");

            migrationBuilder.DropColumn(name: "Subtotal", table: "SalesOrders");

            migrationBuilder.DropColumn(name: "ProductId", table: "OrderProducts");

            migrationBuilder
                .AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:fuzzystrmatch", ",,")
                .OldAnnotation("Npgsql:Enum:payment_method", "cod,vnpay")
                .OldAnnotation(
                    "Npgsql:Enum:sales_order_status",
                    "cancelled,delivered,pending_confirmation,pending_payment,tracking"
                )
                .OldAnnotation("Npgsql:PostgresExtension:fuzzystrmatch", ",,");
        }
    }
}
