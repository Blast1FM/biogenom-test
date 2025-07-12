using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using biogenom_test.Data.Models.PersonalReport.Common;

namespace biogenom_test.Data.Models.PersonalReport.Supplements;

public class SupplementKitNutrient
{
    [Key]
    [Column(Order = 0)]
    public Guid KitID { get; set; }

    [Key]
    [Column(Order = 1)]
    public Guid NutrientID { get; set; }

    [Required]
    // Amount in the unit specified in Nutrient
    public decimal Amount { get; set; } 
    public virtual SupplementKit SupplementKit { get; set; }
    public virtual Nutrient Nutrient { get; set; }
}