using biogenom_test.Data.DTO;
using biogenom_test.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace biogenom_test.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PersonalReportController : ControllerBase
{
    private readonly IPersonalReportService _personalReportService;
    private readonly ISupplementKitRecommendationService _recommendationService;

    public PersonalReportController(IPersonalReportService personalReportService, ISupplementKitRecommendationService recommendationService)
    {
        _personalReportService = personalReportService;
        _recommendationService = recommendationService;
    }

    [HttpPost("consumption")]
    public async Task<IActionResult> UpdateConsumption([FromBody] List<ConsumptionUpdate> consumptionData)
    {
        var result = await _personalReportService.UpdateConsumptionData(consumptionData);
        if (result.IsSuccess)
        {
            return Ok();
        }
        return BadRequest($"Failed to update consumption:{result.ErrorMessage}");
    }

    [HttpPost("recommendedintake")]
    public async Task<IActionResult> UpdateRecommendedIntake([FromBody] List<RecommendedIntakeUpdate> intakeData)
    {
        var result = await _personalReportService.UpdateRecommendedIntake(intakeData);
        if (result.IsSuccess)
        {
            return Ok();
        }
        return BadRequest($"Failed to update recommended intake:{result.ErrorMessage}");
    }

    [HttpGet]
    public async Task<IActionResult> GetPersonalReport()
    {
        var nutrientResult = await _personalReportService.GetNutrientReport();
        if (!nutrientResult.IsSuccess)
        {
            return UnprocessableEntity($"Unable to get nutrient report: {nutrientResult.ErrorMessage}");
        }

        var supplementResult = await _recommendationService.GetSupplementRecommendations();
        if (!supplementResult.IsSuccess)
        {
            return UnprocessableEntity($"Unable to get supplement kit recommendations: {supplementResult.ErrorMessage}");
        }

        var report = new PersonalReport
        {
            Nutrients = nutrientResult.Value,
            RecommendedSupplements = supplementResult.Value
        };

        return Ok(report);
    }
}