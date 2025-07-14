namespace biogenom_test.Data.DTO;

public class NutrientReportItem
{
    public Guid NutrientId { get; set; }
    public string Name { get; set; }
    public string Unit { get; set; }
    public decimal ConsumedAmount { get; set; }
    public decimal RecommendedAmount { get; set; }
}