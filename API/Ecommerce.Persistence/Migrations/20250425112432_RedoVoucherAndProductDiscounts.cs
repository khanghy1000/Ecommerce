using System;
using Ecommerce.Domain;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ecommerce.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RedoVoucherAndProductDiscounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrders_Coupons_CouponId",
                table: "SalesOrders"
            );

            migrationBuilder.DropIndex(name: "IX_SalesOrders_CouponId", table: "SalesOrders");

            migrationBuilder.DropPrimaryKey(name: "PK_Coupons", table: "Coupons");

            migrationBuilder.DropColumn(name: "DiscountPrice", table: "Products");

            migrationBuilder.DropColumn(name: "Description", table: "Coupons");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "Coupons",
                newName: "StartTime"
            );

            migrationBuilder.RenameColumn(
                name: "Multiple",
                table: "Coupons",
                newName: "AllowMultipleUse"
            );

            migrationBuilder.RenameColumn(name: "EndDate", table: "Coupons", newName: "EndTime");

            migrationBuilder.RenameColumn(name: "Id", table: "Coupons", newName: "UsedCount");

            migrationBuilder
                .AlterDatabase()
                .Annotation("Npgsql:Enum:coupon_discount_type", "amount,percent")
                .Annotation("Npgsql:Enum:coupon_type", "product,shipping")
                .Annotation("Npgsql:Enum:payment_method", "cod,vnpay")
                .Annotation(
                    "Npgsql:Enum:sales_order_status",
                    "cancelled,delivered,pending_confirmation,pending_payment,tracking"
                )
                .Annotation("Npgsql:PostgresExtension:fuzzystrmatch", ",,")
                .OldAnnotation("Npgsql:Enum:payment_method", "cod,vnpay")
                .OldAnnotation(
                    "Npgsql:Enum:sales_order_status",
                    "cancelled,delivered,pending_confirmation,pending_payment,tracking"
                )
                .OldAnnotation("Npgsql:PostgresExtension:fuzzystrmatch", ",,");

            migrationBuilder.AddColumn<string>(
                name: "CouponCode",
                table: "SalesOrders",
                type: "character varying(50)",
                nullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Coupons",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255
            );

            migrationBuilder.AlterColumn<bool>(
                name: "Active",
                table: "Coupons",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true
            );

            migrationBuilder
                .AlterColumn<int>(
                    name: "UsedCount",
                    table: "Coupons",
                    type: "integer",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "integer"
                )
                .OldAnnotation(
                    "Npgsql:ValueGenerationStrategy",
                    NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                );

            migrationBuilder.AddColumn<CouponDiscountType>(
                name: "DiscountType",
                table: "Coupons",
                type: "coupon_discount_type",
                nullable: false,
                defaultValue: CouponDiscountType.Percent
            );

            migrationBuilder.AddColumn<decimal>(
                name: "MaxDiscountAmount",
                table: "Coupons",
                type: "numeric",
                nullable: false,
                defaultValue: 0m
            );

            migrationBuilder.AddColumn<int>(
                name: "MaxUseCount",
                table: "Coupons",
                type: "integer",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddColumn<decimal>(
                name: "MinOrderValue",
                table: "Coupons",
                type: "numeric",
                nullable: false,
                defaultValue: 0m
            );

            migrationBuilder.AddColumn<CouponType>(
                name: "Type",
                table: "Coupons",
                type: "coupon_type",
                nullable: false,
                defaultValue: CouponType.Product
            );

            migrationBuilder.AddColumn<string>(
                name: "CouponCode",
                table: "Categories",
                type: "character varying(50)",
                nullable: true
            );

            migrationBuilder.AddPrimaryKey(name: "PK_Coupons", table: "Coupons", column: "Code");

            migrationBuilder.CreateTable(
                name: "ProductDiscounts",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    DiscountPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    StartTime = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    EndTime = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    UpdatedAt = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductDiscounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductDiscounts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_CouponCode",
                table: "SalesOrders",
                column: "CouponCode"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Categories_CouponCode",
                table: "Categories",
                column: "CouponCode"
            );

            migrationBuilder.CreateIndex(
                name: "IX_ProductDiscounts_ProductId",
                table: "ProductDiscounts",
                column: "ProductId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Coupons_CouponCode",
                table: "Categories",
                column: "CouponCode",
                principalTable: "Coupons",
                principalColumn: "Code"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrders_Coupons_CouponCode",
                table: "SalesOrders",
                column: "CouponCode",
                principalTable: "Coupons",
                principalColumn: "Code"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Coupons_CouponCode",
                table: "Categories"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrders_Coupons_CouponCode",
                table: "SalesOrders"
            );

            migrationBuilder.DropTable(name: "ProductDiscounts");

            migrationBuilder.DropIndex(name: "IX_SalesOrders_CouponCode", table: "SalesOrders");

            migrationBuilder.DropPrimaryKey(name: "PK_Coupons", table: "Coupons");

            migrationBuilder.DropIndex(name: "IX_Categories_CouponCode", table: "Categories");

            migrationBuilder.DropColumn(name: "CouponCode", table: "SalesOrders");

            migrationBuilder.DropColumn(name: "DiscountType", table: "Coupons");

            migrationBuilder.DropColumn(name: "MaxDiscountAmount", table: "Coupons");

            migrationBuilder.DropColumn(name: "MaxUseCount", table: "Coupons");

            migrationBuilder.DropColumn(name: "MinOrderValue", table: "Coupons");

            migrationBuilder.DropColumn(name: "Type", table: "Coupons");

            migrationBuilder.DropColumn(name: "CouponCode", table: "Categories");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "Coupons",
                newName: "StartDate"
            );

            migrationBuilder.RenameColumn(name: "EndTime", table: "Coupons", newName: "EndDate");

            migrationBuilder.RenameColumn(
                name: "AllowMultipleUse",
                table: "Coupons",
                newName: "Multiple"
            );

            migrationBuilder.RenameColumn(name: "UsedCount", table: "Coupons", newName: "Id");

            migrationBuilder
                .AlterDatabase()
                .Annotation("Npgsql:Enum:payment_method", "cod,vnpay")
                .Annotation(
                    "Npgsql:Enum:sales_order_status",
                    "cancelled,delivered,pending_confirmation,pending_payment,tracking"
                )
                .Annotation("Npgsql:PostgresExtension:fuzzystrmatch", ",,")
                .OldAnnotation("Npgsql:Enum:coupon_discount_type", "amount,percent")
                .OldAnnotation("Npgsql:Enum:coupon_type", "product,shipping")
                .OldAnnotation("Npgsql:Enum:payment_method", "cod,vnpay")
                .OldAnnotation(
                    "Npgsql:Enum:sales_order_status",
                    "cancelled,delivered,pending_confirmation,pending_payment,tracking"
                )
                .OldAnnotation("Npgsql:PostgresExtension:fuzzystrmatch", ",,");

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPrice",
                table: "Products",
                type: "numeric",
                nullable: true
            );

            migrationBuilder.AlterColumn<bool>(
                name: "Active",
                table: "Coupons",
                type: "boolean",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "boolean"
            );

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Coupons",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50
            );

            migrationBuilder
                .AlterColumn<int>(
                    name: "Id",
                    table: "Coupons",
                    type: "integer",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "integer"
                )
                .Annotation(
                    "Npgsql:ValueGenerationStrategy",
                    NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                );

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Coupons",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddPrimaryKey(name: "PK_Coupons", table: "Coupons", column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_CouponId",
                table: "SalesOrders",
                column: "CouponId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrders_Coupons_CouponId",
                table: "SalesOrders",
                column: "CouponId",
                principalTable: "Coupons",
                principalColumn: "Id"
            );
        }
    }
}
