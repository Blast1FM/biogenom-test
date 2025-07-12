using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using biogenom_test.Data.Models.PersonalReport.Common;

namespace biogenom_test.Data.Models.PersonalReport.Nutrients;

public class CurrentConsumption
{
    [Key]
    [ForeignKey("Nutrient")]
    public Guid NutrientID { get; set; }

    [Required]
    public decimal ConsumedAmount { get; set; } // Amount in the unit specified in Nutrient

    // Navigation property
    public virtual Nutrient Nutrient { get; set; }
}