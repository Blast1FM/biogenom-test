using System.ComponentModel.DataAnnotations;
using biogenom_test.Data.Models.PersonalReport.Nutrients;
using biogenom_test.Data.Models.PersonalReport.Supplements;

namespace biogenom_test.Data.Models.PersonalReport.Common;

public class Nutrient
{
    [Key]
    public Guid NutrientID { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public NutrientAmountUnit Unit { get; set; }

    public virtual RecommendedIntake RecommendedIntake { get; set; }
    public virtual CurrentConsumption CurrentConsumption { get; set; }
    public virtual ICollection<SupplementKitNutrient> SupplementKitNutrients { get; set; }
}