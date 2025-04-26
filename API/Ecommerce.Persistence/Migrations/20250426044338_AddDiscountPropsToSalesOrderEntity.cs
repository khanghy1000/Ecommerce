using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscountPropsToSalesOrderEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrders_Coupons_CouponCode",
                table: "SalesOrders"
            );

            migrationBuilder.DropIndex(name: "IX_SalesOrders_CouponCode", table: "SalesOrders");

            migrationBuilder.DropColumn(name: "CouponCode", table: "SalesOrders");

            migrationBuilder.AddColumn<decimal>(
                name: "ProductDiscountAmount",
                table: "SalesOrders",
                type: "numeric",
                nullable: false,
                defaultValue: 0m
            );

            migrationBuilder.AddColumn<decimal>(
                name: "ShippingDiscountAmount",
                table: "SalesOrders",
                type: "numeric",
                nullable: false,
                defaultValue: 0m
            );

            migrationBuilder.AlterColumn<bool>(
                name: "Active",
                table: "Products",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean"
            );

            migrationBuilder.AlterColumn<int>(
                name: "UsedCount",
                table: "Coupons",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer"
            );

            migrationBuilder.AlterColumn<bool>(
                name: "Active",
                table: "Coupons",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean"
            );

            migrationBuilder.CreateTable(
                name: "CouponSalesOrder",
                columns: table => new
                {
                    CouponsCode = table.Column<string>(
                        type: "character varying(50)",
                        nullable: false
                    ),
                    SalesOrdersId = table.Column<int>(type: "integer", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_CouponSalesOrder",
                        x => new { x.CouponsCode, x.SalesOrdersId }
                    );
                    table.ForeignKey(
                        name: "FK_CouponSalesOrder_Coupons_CouponsCode",
                        column: x => x.CouponsCode,
                        principalTable: "Coupons",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_CouponSalesOrder_SalesOrders_SalesOrdersId",
                        column: x => x.SalesOrdersId,
                        principalTable: "SalesOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_CouponSalesOrder_SalesOrdersId",
                table: "CouponSalesOrder",
                column: "SalesOrdersId"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "CouponSalesOrder");

            migrationBuilder.DropColumn(name: "ProductDiscountAmount", table: "SalesOrders");

            migrationBuilder.DropColumn(name: "ShippingDiscountAmount", table: "SalesOrders");

            migrationBuilder.AddColumn<string>(
                name: "CouponCode",
                table: "SalesOrders",
                type: "character varying(50)",
                nullable: true
            );

            migrationBuilder.AlterColumn<bool>(
                name: "Active",
                table: "Products",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true
            );

            migrationBuilder.AlterColumn<int>(
                name: "UsedCount",
                table: "Coupons",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0
            );

            migrationBuilder.AlterColumn<bool>(
                name: "Active",
                table: "Coupons",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_CouponCode",
                table: "SalesOrders",
                column: "CouponCode"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrders_Coupons_CouponCode",
                table: "SalesOrders",
                column: "CouponCode",
                principalTable: "Coupons",
                principalColumn: "Code"
            );
        }
    }
}
