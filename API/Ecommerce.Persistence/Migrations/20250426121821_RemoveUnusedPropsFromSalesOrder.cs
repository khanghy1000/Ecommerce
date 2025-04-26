using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedPropsFromSalesOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "CouponId", table: "SalesOrders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CouponId",
                table: "SalesOrders",
                type: "integer",
                nullable: true
            );
        }
    }
}
