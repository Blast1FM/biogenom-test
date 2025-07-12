using System.ComponentModel.DataAnnotations;

namespace biogenom_test.Data.Models.PersonalReport.Supplements;

public class SupplementKit
{
    [Key]
    public Guid KitID { get; set; }

    [Required]
    public string Name { get; set; }

    public virtual ICollection<SupplementKitNutrient> SupplementKitNutrients { get; set; }
}
