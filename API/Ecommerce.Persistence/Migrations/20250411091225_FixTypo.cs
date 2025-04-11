using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixTypo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductSubCategories_Products_ProductId",
                table: "ProductSubCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductSubCategories_Subcategories_SubCategoryId",
                table: "ProductSubCategories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductSubCategories",
                table: "ProductSubCategories");

            migrationBuilder.RenameTable(
                name: "ProductSubCategories",
                newName: "ProductSubcategories");

            migrationBuilder.RenameColumn(
                name: "SubCategoryId",
                table: "ProductSubcategories",
                newName: "SubcategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductSubCategories_SubCategoryId",
                table: "ProductSubcategories",
                newName: "IX_ProductSubcategories_SubcategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductSubcategories",
                table: "ProductSubcategories",
                columns: new[] { "ProductId", "SubcategoryId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ProductSubcategories_Products_ProductId",
                table: "ProductSubcategories",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductSubcategories_Subcategories_SubcategoryId",
                table: "ProductSubcategories",
                column: "SubcategoryId",
                principalTable: "Subcategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductSubcategories_Products_ProductId",
                table: "ProductSubcategories");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductSubcategories_Subcategories_SubcategoryId",
                table: "ProductSubcategories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductSubcategories",
                table: "ProductSubcategories");

            migrationBuilder.RenameTable(
                name: "ProductSubcategories",
                newName: "ProductSubCategories");

            migrationBuilder.RenameColumn(
                name: "SubcategoryId",
                table: "ProductSubCategories",
                newName: "SubCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductSubcategories_SubcategoryId",
                table: "ProductSubCategories",
                newName: "IX_ProductSubCategories_SubCategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductSubCategories",
                table: "ProductSubCategories",
                columns: new[] { "ProductId", "SubCategoryId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ProductSubCategories_Products_ProductId",
                table: "ProductSubCategories",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductSubCategories_Subcategories_SubCategoryId",
                table: "ProductSubCategories",
                column: "SubCategoryId",
                principalTable: "Subcategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
