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
        private readonly ILogger<SupplementKitRecommendationService> _logger;

        public SupplementKitRecommendationService(PersonalReportContext context, ILogger<SupplementKitRecommendationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<OperationResult<List<SupplementKitReportItem>>> GetSupplementRecommendations()
        {
            _logger.LogInformation("Starting GetSupplementRecommendations.");

            try
            {
                // Retrieve nutrient data with recommended intakes and current consumption
                _logger.LogDebug("Querying nutrients with recommended intakes.");
                var nutrients = await _context.Nutrients
                    .Include(n => n.RecommendedIntake)
                    .Include(n => n.CurrentConsumption)
                    .Where(n => n.RecommendedIntake != null)
                    .ToListAsync();

                if (!nutrients.Any())
                {
                    _logger.LogDebug("No nutrients with recommended intakes found. Returning empty recommendations.");
                    return OperationResult<List<SupplementKitReportItem>>.Success(new List<SupplementKitReportItem>());
                }
                _logger.LogDebug($"Retrieved {nutrients.Count} nutrients with recommended intakes.");

                // Create lookup for nutrient metadata (Name, Unit)
                _logger.LogDebug("Creating nutrient lookup dictionary.");
                var nutrientLookup = nutrients.ToDictionary(n => n.NutrientID, n => new { n.Name, n.Unit });

                // Initialize dictionaries for current consumption (C) and recommended amounts (R)
                _logger.LogDebug("Initializing consumption and recommended dictionaries.");
                var C = nutrients.ToDictionary(n => n.NutrientID, n => n.CurrentConsumption?.ConsumedAmount ?? 0);
                var R = nutrients.ToDictionary(n => n.NutrientID, n => n.RecommendedIntake.RecommendedAmount);

                // Retrieve supplement kit data
                _logger.LogDebug("Querying supplement kits.");
                var kits = await _context.SupplementKits
                    .Include(sk => sk.SupplementKitNutrients)
                    .ThenInclude(skn => skn.Nutrient)
                    .ToListAsync();

                if (!kits.Any())
                {
                    _logger.LogDebug("No supplement kits found. Returning empty recommendations.");
                    return OperationResult<List<SupplementKitReportItem>>.Success(new List<SupplementKitReportItem>());
                }
                _logger.LogDebug($"Retrieved {kits.Count} supplement kits.");

                // Initialize total intake (T) as current consumption
                var T = new Dictionary<Guid, decimal>(C);
                var currentDeviation = CalculateDeviation(T, R);
                _logger.LogDebug($"Initialized total intake with current deviation: {currentDeviation}.");

                // Greedy selection of up to 3 kits
                var selectedKits = new List<SupplementKit>();
                var availableKits = kits.ToList();

                for (int i = 0; i < 3; i++)
                {
                    _logger.LogDebug($"Starting greedy selection iteration {i + 1}.");
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
                        _logger.LogDebug($"Selected kit {bestKit.Name} with deviation {minDeviation}.");
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
                        _logger.LogDebug("No kit improves deviation. Stopping greedy selection.");
                        break;
                    }
                }

                // Convert selected kits to DTOs
                _logger.LogDebug($"Converting {selectedKits.Count} selected kits to DTOs.");
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

                _logger.LogInformation($"GetSupplementRecommendations completed successfully with {reportItems.Count} recommendations.");
                return OperationResult<List<SupplementKitReportItem>>.Success(reportItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating supplement recommendations.");
                return OperationResult<List<SupplementKitReportItem>>.Failure($"An error occurred while generating supplement recommendations: {ex.Message}");
            }
        }

        private decimal CalculateDeviation(Dictionary<Guid, decimal> T, Dictionary<Guid, decimal> R)
        {
            return R.Sum(r => Math.Abs((T.ContainsKey(r.Key) ? T[r.Key] : 0) - r.Value));
        }
    }
}