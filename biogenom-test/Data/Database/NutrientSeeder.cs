using biogenom_test.Data.Models.PersonalReport.Common;
using biogenom_test.Data.Models.PersonalReport.Nutrients;
using Microsoft.EntityFrameworkCore;

namespace biogenom_test.Data.Database;

public class NutrientSeeder
{
    public static void SeedData(ModelBuilder modelBuilder)
    {
        var proteinId = new Guid("d2834471-2a0d-4307-b7e9-a175d129a011");
        var vitaminCId = new Guid("e3934471-2a0d-4307-b7e9-a175d129a012");
        var vitaminDId = new Guid("d0e85755-1807-4d51-9edf-dbd110d2c5b2");
        var waterId = new Guid("5cbc1313-698a-46ea-91a4-06cc601e8a16");
        var zincId = new Guid("43e5a276-4d69-4e6a-a846-43138ff43025");

        modelBuilder.Entity<Nutrient>().HasData(
            new Nutrient { NutrientID = proteinId, Name = "Protein", Unit = NutrientAmountUnit.Gram },
            new Nutrient { NutrientID = vitaminCId, Name = "Vitamin C", Unit = NutrientAmountUnit.Milligram },
            new Nutrient {NutrientID = vitaminDId, Name = "Vitamin D", Unit = NutrientAmountUnit.Microgram},
            new Nutrient {NutrientID = waterId, Name = "Water", Unit = NutrientAmountUnit.Gram},
            new Nutrient {NutrientID = zincId, Name = "Zinc", Unit = NutrientAmountUnit.Milligram}

        );

        modelBuilder.Entity<RecommendedIntake>().HasData(
            new RecommendedIntake { NutrientID = proteinId, RecommendedAmount = 50 },
            new RecommendedIntake { NutrientID = vitaminCId, RecommendedAmount = 90 },
            new RecommendedIntake {NutrientID = vitaminDId, RecommendedAmount = 20},
            new RecommendedIntake {NutrientID = waterId, RecommendedAmount = 2000},
            new RecommendedIntake {NutrientID = zincId, RecommendedAmount = 11}
        );
    }
}