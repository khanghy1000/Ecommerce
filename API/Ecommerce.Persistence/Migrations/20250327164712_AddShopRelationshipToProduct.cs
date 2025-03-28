using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddShopRelationshipToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShopId",
                table: "Products",
                type: "character varying(36)",
                maxLength: 36,
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.CreateIndex(
                name: "IX_Products_ShopId",
                table: "Products",
                column: "ShopId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Products_AspNetUsers_ShopId",
                table: "Products",
                column: "ShopId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_AspNetUsers_ShopId",
                table: "Products"
            );

            migrationBuilder.DropIndex(name: "IX_Products_ShopId", table: "Products");

            migrationBuilder.DropColumn(name: "ShopId", table: "Products");
        }
    }
}
