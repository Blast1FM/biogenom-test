using biogenom_test.Common;
using biogenom_test.Data.Database;
using biogenom_test.Data.DTO;
using biogenom_test.Data.Models.PersonalReport.Nutrients;
using Microsoft.EntityFrameworkCore;

namespace biogenom_test.Services;

public class PersonalReportService : IPersonalReportService
{
    private readonly PersonalReportContext _context;

    public PersonalReportService(PersonalReportContext context)
    {
        _context = context;
    }

    public async Task<OperationResult<Unit>> UpdateConsumptionData(List<ConsumptionUpdate> consumptionData)
    {
        if (consumptionData == null || !consumptionData.Any())
        {
            return OperationResult<Unit>.Failure("Input list is null or empty.");
        }

        // Validate NutrientIDs
        var nutrientIds = consumptionData.Select(cd => cd.NutrientId).ToHashSet();
        var validNutrientIds = await _context.Nutrients
            .Where(n => nutrientIds.Contains(n.NutrientID))
            .Select(n => n.NutrientID)
            .ToListAsync();

        var invalidIds = nutrientIds.Except(validNutrientIds).ToList();

        if (invalidIds.Any())
        {
            return OperationResult<Unit>.Failure($"Invalid Nutrient IDs: {string.Join(", ", invalidIds)}");
        }

        // Validate ConsumedAmount
        if (consumptionData.Any(cd => cd.ConsumedAmount < 0))
        {
            return OperationResult<Unit>.Failure("Consumed amounts cannot be negative.");
        }

        try
        {
            // Clear existing records
            await _context.CurrentConsumptions.ExecuteDeleteAsync();

            // Map DTOs to entities
            var newConsumptions = consumptionData.Select(cd => new CurrentConsumption
            {
                NutrientID = cd.NutrientId,
                ConsumedAmount = cd.ConsumedAmount
            }).ToList();

            // Add new records
            await _context.CurrentConsumptions.AddRangeAsync(newConsumptions);

            // Save changes
            await _context.SaveChangesAsync();

            return OperationResult<Unit>.Success(Unit.Value);
        }
        catch (DbUpdateException ex)
        {
            return OperationResult<Unit>.Failure($"Database update failed: {ex.InnerException?.Message ?? ex.Message}");
        }
        catch (Exception ex)
        {
            return OperationResult<Unit>.Failure($"An unexpected error occurred: {ex.Message}");
        }
    }

    public async Task<OperationResult<Unit>> UpdateRecommendedIntake(List<RecommendedIntakeUpdate> intakeData)
    {
        if (intakeData == null || !intakeData.Any())
        {
            return OperationResult<Unit>.Failure("Input list is null or empty.");
        }

        if (intakeData.Any(id => id == null))
        {
            return OperationResult<Unit>.Failure("Input list contains null items.");
        }

        // Validate NutrientIDs
        var nutrientIds = intakeData.Select(id => id.NutrientId).Distinct().ToList();
        var validNutrientIds = await _context.Nutrients
            .Where(n => nutrientIds.Contains(n.NutrientID))
            .Select(n => n.NutrientID)
            .ToListAsync();

        var invalidIds = nutrientIds.Except(validNutrientIds).ToList();
        if (invalidIds.Any())
        {
            return OperationResult<Unit>.Failure($"Invalid Nutrient IDs: {string.Join(", ", invalidIds)}");
        }

        // Check for duplicates
        if (intakeData.Count > nutrientIds.Count)
        {
            return OperationResult<Unit>.Failure("Too many nutrients listed.");
        }

        // Validate RecommendedAmount
        if (intakeData.Any(id => id.RecommendedAmount < 0))
        {
            return OperationResult<Unit>.Failure("Recommended amounts cannot be negative.");
        }

        try
        {
            // Load existing recommended intakes into a dictionary for quick lookup
            var existingIntakes = await _context.RecommendedIntakes.ToDictionaryAsync(ri => ri.NutrientID);

            foreach (var update in intakeData)
            {
                if (existingIntakes.TryGetValue(update.NutrientId, out var existing))
                {
                    // Update existing record
                    existing.RecommendedAmount = update.RecommendedAmount;
                }
                else
                {
                    // Add new record
                    var newIntake = new RecommendedIntake
                    {
                        NutrientID = update.NutrientId,
                        RecommendedAmount = update.RecommendedAmount
                    };
                    _context.RecommendedIntakes.Add(newIntake);
                }
            }

            // Save changes to the database
            await _context.SaveChangesAsync();

            return OperationResult<Unit>.Success(Unit.Value);
        }
        catch (DbUpdateException ex)
        {
            return OperationResult<Unit>.Failure($"Database update failed: {ex.InnerException?.Message ?? ex.Message}");
        }
        catch (Exception ex)
        {
            return OperationResult<Unit>.Failure($"An unexpected error occurred: {ex.Message}");
        }
    }

    public async Task<OperationResult<List<NutrientReportItem>>> GetNutrientReport()
    {
        try
        {
            // Query for all nutrients that have a RecommendedIntake
            var nutrients = await _context.Nutrients
                .Include(n => n.RecommendedIntake)
                .Include(n => n.CurrentConsumption)
                .Where(n => n.RecommendedIntake != null)
                .ToListAsync();

            // Create NutrientReportItem DTOs
            var reportItems = nutrients.Select(n => new NutrientReportItem
            {
                NutrientId = n.NutrientID,
                Name = n.Name,
                Unit = Enum.GetName(n.Unit) ?? string.Empty,
                RecommendedAmount = n.RecommendedIntake.RecommendedAmount,
                ConsumedAmount = n.CurrentConsumption?.ConsumedAmount ?? 0
            }).ToList();

            return OperationResult<List<NutrientReportItem>>.Success(reportItems);
        }
        catch (Exception ex)
        {
            return OperationResult<List<NutrientReportItem>>.Failure($"An error occurred while generating the nutrient report: {ex.Message}");
        }
    }

}