using Microsoft.EntityFrameworkCore;
using biogenom_test.Data.Database;
using biogenom_test.Common;
using biogenom_test.Data.DTO;
using biogenom_test.Data.Models.PersonalReport.Supplements;

namespace biogenom_test.Services
{
    public class SupplementKitRecommendationService : ISupplementKitRecommendationService
    {
        private readonly PersonalReportContext _context;

        public SupplementKitRecommendationService(PersonalReportContext context)
        {
            _context = context;
        }

        public async Task<OperationResult<List<SupplementKitReportItem>>> GetSupplementRecommendations()
        {
            try
            {
                // Retrieve nutrient data with recommended intakes and current consumption
                var nutrients = await _context.Nutrients
                    .Include(n => n.RecommendedIntake)
                    .Include(n => n.CurrentConsumption)
                    .Where(n => n.RecommendedIntake != null)
                    .ToListAsync();

                if (!nutrients.Any())
                {
                    return OperationResult<List<SupplementKitReportItem>>.Success(new List<SupplementKitReportItem>());
                }

                // Create lookup for nutrient metadata (Name, Unit)
                var nutrientLookup = nutrients.ToDictionary(n => n.NutrientID, n => new { n.Name, n.Unit });

                // Initialize dictionaries for current consumption (C) and recommended amounts (R)
                var C = nutrients.ToDictionary(n => n.NutrientID, n => n.CurrentConsumption?.ConsumedAmount ?? 0);
                var R = nutrients.ToDictionary(n => n.NutrientID, n => n.RecommendedIntake.RecommendedAmount);

                // Retrieve supplement kit data
                var kits = await _context.SupplementKits
                    .Include(sk => sk.SupplementKitNutrients)
                    .ThenInclude(skn => skn.Nutrient)
                    .ToListAsync();

                if (!kits.Any())
                {
                    return OperationResult<List<SupplementKitReportItem>>.Success(new List<SupplementKitReportItem>());
                }

                // Initialize total intake (T) as current consumption
                var T = new Dictionary<Guid, decimal>(C);
                var currentDeviation = CalculateDeviation(T, R);

                // Greedy selection of up to 3 kits
                var selectedKits = new List<SupplementKit>();
                var availableKits = kits.ToList();

                for (int i = 0; i < 3; i++)
                {
                    SupplementKit bestKit = null;
                    decimal minDeviation = decimal.MaxValue;

                    foreach (var kit in availableKits)
                    {
                        var tempT = new Dictionary<Guid, decimal>(T);
                        foreach (var nutAmt in kit.SupplementKitNutrients)
                        {
                            tempT[nutAmt.NutrientID] = tempT.ContainsKey(nutAmt.NutrientID) ? tempT[nutAmt.NutrientID] + nutAmt.Amount : nutAmt.Amount;
                        }

                        var deviation = CalculateDeviation(tempT, R);
                        if (deviation < minDeviation)
                        {
                            minDeviation = deviation;
                            bestKit = kit;
                        }
                    }

                    if (bestKit != null && minDeviation < currentDeviation)
                    {
                        selectedKits.Add(bestKit);
                        availableKits.Remove(bestKit);
                        T = new Dictionary<Guid, decimal>(T);
                        foreach (var nutAmt in bestKit.SupplementKitNutrients)
                        {
                            T[nutAmt.NutrientID] = T.ContainsKey(nutAmt.NutrientID) ? T[nutAmt.NutrientID] + nutAmt.Amount : nutAmt.Amount;
                        }
                        currentDeviation = minDeviation;
                    }
                    else
                    {
                        break;
                    }
                }

                // Convert selected kits to DTOs
                var reportItems = selectedKits.Select(kit => new SupplementKitReportItem
                {
                    KitId = kit.KitID,
                    Name = kit.Name,
                    Nutrients = kit.SupplementKitNutrients
                        .Where(skn => nutrientLookup.ContainsKey(skn.NutrientID))
                        .Select(skn => new SupplementKitNutrientReportItem
                        {
                            NutrientId = skn.NutrientID,
                            NutrientName = nutrientLookup[skn.NutrientID].Name,
                            Unit = Enum.GetName(nutrientLookup[skn.NutrientID].Unit) ?? string.Empty,
                            Amount = skn.Amount
                        }).ToList()
                }).ToList();

                return OperationResult<List<SupplementKitReportItem>>.Success(reportItems);
            }
            catch (Exception ex)
            {
                return OperationResult<List<SupplementKitReportItem>>.Failure($"An error occurred while generating supplement recommendations: {ex.Message}");
            }
        }

        private decimal CalculateDeviation(Dictionary<Guid, decimal> T, Dictionary<Guid, decimal> R)
        {
            return R.Sum(r => Math.Abs((T.ContainsKey(r.Key) ? T[r.Key] : 0) - r.Value));
        }
    }
}