using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace biogenom_test.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Nutrients",
                columns: table => new
                {
                    NutrientID = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Unit = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nutrients", x => x.NutrientID);
                });

            migrationBuilder.CreateTable(
                name: "SupplementKits",
                columns: table => new
                {
                    KitID = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplementKits", x => x.KitID);
                });

            migrationBuilder.CreateTable(
                name: "CurrentConsumptions",
                columns: table => new
                {
                    NutrientID = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsumedAmount = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrentConsumptions", x => x.NutrientID);
                    table.ForeignKey(
                        name: "FK_CurrentConsumptions_Nutrients_NutrientID",
                        column: x => x.NutrientID,
                        principalTable: "Nutrients",
                        principalColumn: "NutrientID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecommendedIntakes",
                columns: table => new
                {
                    NutrientID = table.Column<Guid>(type: "uuid", nullable: false),
                    RecommendedAmount = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecommendedIntakes", x => x.NutrientID);
                    table.ForeignKey(
                        name: "FK_RecommendedIntakes_Nutrients_NutrientID",
                        column: x => x.NutrientID,
                        principalTable: "Nutrients",
                        principalColumn: "NutrientID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplementKitNutrients",
                columns: table => new
                {
                    KitID = table.Column<Guid>(type: "uuid", nullable: false),
                    NutrientID = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplementKitNutrients", x => new { x.KitID, x.NutrientID });
                    table.ForeignKey(
                        name: "FK_SupplementKitNutrients_Nutrients_NutrientID",
                        column: x => x.NutrientID,
                        principalTable: "Nutrients",
                        principalColumn: "NutrientID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupplementKitNutrients_SupplementKits_KitID",
                        column: x => x.KitID,
                        principalTable: "SupplementKits",
                        principalColumn: "KitID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Nutrients",
                columns: new[] { "NutrientID", "Name", "Unit" },
                values: new object[,]
                {
                    { new Guid("43e5a276-4d69-4e6a-a846-43138ff43025"), "Zinc", "Milligram" },
                    { new Guid("5cbc1313-698a-46ea-91a4-06cc601e8a16"), "Water", "Gram" },
                    { new Guid("d0e85755-1807-4d51-9edf-dbd110d2c5b2"), "Vitamin D", "Microgram" },
                    { new Guid("d2834471-2a0d-4307-b7e9-a175d129a011"), "Protein", "Gram" },
                    { new Guid("e3934471-2a0d-4307-b7e9-a175d129a012"), "Vitamin C", "Milligram" }
                });

            migrationBuilder.InsertData(
                table: "RecommendedIntakes",
                columns: new[] { "NutrientID", "RecommendedAmount" },
                values: new object[,]
                {
                    { new Guid("43e5a276-4d69-4e6a-a846-43138ff43025"), 11m },
                    { new Guid("5cbc1313-698a-46ea-91a4-06cc601e8a16"), 2000m },
                    { new Guid("d0e85755-1807-4d51-9edf-dbd110d2c5b2"), 20m },
                    { new Guid("d2834471-2a0d-4307-b7e9-a175d129a011"), 50m },
                    { new Guid("e3934471-2a0d-4307-b7e9-a175d129a012"), 90m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_SupplementKitNutrients_NutrientID",
                table: "SupplementKitNutrients",
                column: "NutrientID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CurrentConsumptions");

            migrationBuilder.DropTable(
                name: "RecommendedIntakes");

            migrationBuilder.DropTable(
                name: "SupplementKitNutrients");

            migrationBuilder.DropTable(
                name: "Nutrients");

            migrationBuilder.DropTable(
                name: "SupplementKits");
        }
    }
}
