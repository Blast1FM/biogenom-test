using biogenom_test.Data.Models.PersonalReport.Common;
using biogenom_test.Data.Models.PersonalReport.Nutrients;
using biogenom_test.Data.Models.PersonalReport.Supplements;
using Microsoft.EntityFrameworkCore;

namespace biogenom_test.Data.Database;

public class PersonalReportContext : DbContext
{
    public DbSet<Nutrient> Nutrients { get; set; }
    public DbSet<RecommendedIntake> RecommendedIntakes { get; set; }
    public DbSet<CurrentConsumption> CurrentConsumptions { get; set; }
    public DbSet<SupplementKit> SupplementKits { get; set; }
    public DbSet<SupplementKitNutrient> SupplementKitNutrients { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RecommendedIntake>()
            .HasOne(ri => ri.Nutrient)
            .WithOne(n => n.RecommendedIntake)
            .HasForeignKey<RecommendedIntake>(ri => ri.NutrientID);

        modelBuilder.Entity<CurrentConsumption>()
            .HasOne(cc => cc.Nutrient)
            .WithOne(n => n.CurrentConsumption)
            .HasForeignKey<CurrentConsumption>(cc => cc.NutrientID);

        modelBuilder.Entity<SupplementKitNutrient>()
            .HasKey(skn => new { skn.KitID, skn.NutrientID });

        modelBuilder.Entity<SupplementKitNutrient>()
            .HasOne(skn => skn.SupplementKit)
            .WithMany(sk => sk.SupplementKitNutrients)
            .HasForeignKey(skn => skn.KitID);

        modelBuilder.Entity<SupplementKitNutrient>()
            .HasOne(skn => skn.Nutrient)
            .WithMany(n => n.SupplementKitNutrients)
            .HasForeignKey(skn => skn.NutrientID);

        modelBuilder.Entity<Nutrient>()
            .Property(n => n.Unit)
            .HasConversion<string>();
    }
}