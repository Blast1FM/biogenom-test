namespace biogenom_test.Data.DTO;
public class SupplementKitReportItem
{
    public Guid KitId { get; set; }
    public string Name { get; set; }
    public List<SupplementKitNutrientReportItem> Nutrients { get; set; }
}