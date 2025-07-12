using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using biogenom_test.Data.Models.PersonalReport.Common;

namespace biogenom_test.Data.Models.PersonalReport.Nutrients;

public class RecommendedIntake
{
    [Key]
    [ForeignKey("Nutrient")]
    public Guid NutrientID { get; set; }

    [Required]
    public decimal RecommendedAmount { get; set; } // Amount in the unit specified in Nutrient

    // Navigation property
    public virtual Nutrient Nutrient { get; set; }
}