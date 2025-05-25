using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixCategoryCouponRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Coupons_CouponCode",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_CouponCode",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "CouponCode",
                table: "Categories");

            migrationBuilder.CreateTable(
                name: "CategoryCoupon",
                columns: table => new
                {
                    CategoriesId = table.Column<int>(type: "integer", nullable: false),
                    CouponsCode = table.Column<string>(type: "character varying(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryCoupon", x => new { x.CategoriesId, x.CouponsCode });
                    table.ForeignKey(
                        name: "FK_CategoryCoupon_Categories_CategoriesId",
                        column: x => x.CategoriesId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CategoryCoupon_Coupons_CouponsCode",
                        column: x => x.CouponsCode,
                        principalTable: "Coupons",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CategoryCoupon_CouponsCode",
                table: "CategoryCoupon",
                column: "CouponsCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategoryCoupon");

            migrationBuilder.AddColumn<string>(
                name: "CouponCode",
                table: "Categories",
                type: "character varying(50)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_CouponCode",
                table: "Categories",
                column: "CouponCode");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Coupons_CouponCode",
                table: "Categories",
                column: "CouponCode",
                principalTable: "Coupons",
                principalColumn: "Code");
        }
    }
}
