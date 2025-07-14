using biogenom_test.Common;
using biogenom_test.Data.DTO;

namespace biogenom_test.Services;

public interface IPersonalReportService
{
    public Task<OperationResult<Unit>> UpdateConsumptionData(List<ConsumptionUpdate> consumptionData);
    public Task<OperationResult<Unit>> UpdateRecommendedIntake(List<RecommendedIntakeUpdate> intakeData);
    public Task<OperationResult<List<NutrientReportItem>>> GetNutrientReport();

}