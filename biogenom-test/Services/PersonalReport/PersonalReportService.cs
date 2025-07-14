using biogenom_test.Common;
using biogenom_test.Data.Database;
using biogenom_test.Data.DTO;
using biogenom_test.Data.Models.PersonalReport.Nutrients;
using Microsoft.EntityFrameworkCore;

namespace biogenom_test.Services;

public class PersonalReportService : IPersonalReportService
{
    private readonly PersonalReportContext _context;
    private readonly ILogger<PersonalReportService> _logger;

    public PersonalReportService(PersonalReportContext context, ILogger<PersonalReportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<OperationResult<Unit>> UpdateConsumptionData(List<ConsumptionUpdate> consumptionData)
    {
        _logger.LogDebug($"Starting UpdateConsumptionData with {consumptionData?.Count ?? 0} items.");
        if (consumptionData == null || !consumptionData.Any())
        {
            _logger.LogWarning("Input list is null or empty.");
            return OperationResult<Unit>.Failure("Input list is null or empty.");
        }

        // Validate NutrientIDs
        var nutrientIds = consumptionData.Select(cd => cd.NutrientId).ToHashSet();
        _logger.LogDebug($"Validating {nutrientIds.Count} nutrient IDs.");
        var validNutrientIds = await _context.Nutrients
            .Where(n => nutrientIds.Contains(n.NutrientID))
            .Select(n => n.NutrientID)
            .ToListAsync();

        var invalidIds = nutrientIds.Except(validNutrientIds).ToList();

        if (invalidIds.Any())
        {
            _logger.LogWarning("Invalid Nutrient IDs found: {InvalidIds}", string.Join(", ", invalidIds));
            return OperationResult<Unit>.Failure($"Invalid Nutrient IDs: {string.Join(", ", invalidIds)}");
        }

        if (consumptionData.Any(cd => cd.ConsumedAmount < 0))
        {
            _logger.LogWarning("Negative consumed amounts detected.");
            return OperationResult<Unit>.Failure("Consumed amounts cannot be negative.");
        }

        try
        {
            _logger.LogDebug("Clearing existing CurrentConsumption records.");
            await _context.CurrentConsumptions.ExecuteDeleteAsync();

            var newConsumptions = consumptionData.Select(cd => new CurrentConsumption
            {
                NutrientID = cd.NutrientId,
                ConsumedAmount = cd.ConsumedAmount
            }).ToList();

            _logger.LogDebug($"Adding {newConsumptions.Count} new CurrentConsumption records.");
            await _context.CurrentConsumptions.AddRangeAsync(newConsumptions);

            await _context.SaveChangesAsync();

            _logger.LogInformation("UpdateConsumptionData completed successfully.");
            return OperationResult<Unit>.Success(Unit.Value);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database update failed in UpdateConsumptionData.");
            return OperationResult<Unit>.Failure($"Database update failed: {ex.InnerException?.Message ?? ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred in UpdateConsumptionData.");
            return OperationResult<Unit>.Failure($"An unexpected error occurred: {ex.Message}");
        }
    }

   public async Task<OperationResult<Unit>> UpdateRecommendedIntake(List<RecommendedIntakeUpdate> intakeData)
    {
        _logger.LogInformation($"Starting UpdateRecommendedIntake with {intakeData?.Count ?? 0} items.");

        if (intakeData == null || !intakeData.Any())
        {
            _logger.LogWarning("Input list is null or empty.");
            return OperationResult<Unit>.Failure("Input list is null or empty.");
        }

        if (intakeData.Any(id => id == null))
        {
            _logger.LogWarning("Input list contains null items.");
            return OperationResult<Unit>.Failure("Input list contains null items.");
        }

        // Validate NutrientIDs
        var nutrientIds = intakeData.Select(id => id.NutrientId).Distinct().ToList();
        _logger.LogDebug("Validating {Count} distinct nutrient IDs.", nutrientIds.Count);
        var validNutrientIds = await _context.Nutrients
            .Where(n => nutrientIds.Contains(n.NutrientID))
            .Select(n => n.NutrientID)
            .ToListAsync();

        var invalidIds = nutrientIds.Except(validNutrientIds).ToList();
        if (invalidIds.Any())
        {
            _logger.LogWarning($"Invalid Nutrient IDs found: {string.Join(", ", invalidIds)}");
            return OperationResult<Unit>.Failure($"Invalid Nutrient IDs: {string.Join(", ", invalidIds)}");
        }

        if (intakeData.Count > nutrientIds.Count)
        {
            _logger.LogWarning("Duplicate nutrient IDs detected in input data.");
            return OperationResult<Unit>.Failure("Too many nutrients listed.");
        }

        if (intakeData.Any(id => id.RecommendedAmount < 0))
        {
            _logger.LogWarning("Negative recommended amounts detected.");
            return OperationResult<Unit>.Failure("Recommended amounts cannot be negative.");
        }

        try
        {
            _logger.LogDebug("Loading existing RecommendedIntake records.");
            var existingIntakes = await _context.RecommendedIntakes.ToDictionaryAsync(ri => ri.NutrientID);

            foreach (var update in intakeData)
            {
                if (existingIntakes.TryGetValue(update.NutrientId, out var existing))
                {
                    _logger.LogDebug($"Updating RecommendedIntake for Nutrient ID: {update.NutrientId}");
                    existing.RecommendedAmount = update.RecommendedAmount;
                }
                else
                {
                    _logger.LogDebug($"Adding new RecommendedIntake for Nutrient ID: {update.NutrientId}");
                    var newIntake = new RecommendedIntake
                    {
                        NutrientID = update.NutrientId,
                        RecommendedAmount = update.RecommendedAmount
                    };
                    _context.RecommendedIntakes.Add(newIntake);
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("UpdateRecommendedIntake completed successfully.");
            return OperationResult<Unit>.Success(Unit.Value);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database update failed in UpdateRecommendedIntake.");
            return OperationResult<Unit>.Failure($"Database update failed: {ex.InnerException?.Message ?? ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred in UpdateRecommendedIntake.");
            return OperationResult<Unit>.Failure($"An unexpected error occurred: {ex.Message}");
        }
    }

    public async Task<OperationResult<List<NutrientReportItem>>> GetNutrientReport()
    {
        _logger.LogInformation("Starting GetNutrientReport.");

        try
        {
            _logger.LogDebug("Querying nutrients with RecommendedIntake.");
            var nutrients = await _context.Nutrients
                .Include(n => n.RecommendedIntake)
                .Include(n => n.CurrentConsumption)
                .Where(n => n.RecommendedIntake != null)
                .ToListAsync();

            _logger.LogDebug($"Creating NutrientReportItem DTOs for {nutrients.Count} nutrients.");
            var reportItems = nutrients.Select(n => new NutrientReportItem
            {
                NutrientId = n.NutrientID,
                Name = n.Name,
                Unit = Enum.GetName(n.Unit) ?? string.Empty,
                RecommendedAmount = n.RecommendedIntake.RecommendedAmount,
                ConsumedAmount = n.CurrentConsumption?.ConsumedAmount ?? 0
            }).ToList();

            _logger.LogInformation("GetNutrientReport completed successfully.");
            return OperationResult<List<NutrientReportItem>>.Success(reportItems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while generating the nutrient report.");
            return OperationResult<List<NutrientReportItem>>.Failure($"An error occurred while generating the nutrient report: {ex.Message}");
        }
    }
}