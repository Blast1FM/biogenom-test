using biogenom_test.Common;
using biogenom_test.Data.DTO;

namespace biogenom_test.Services;

public interface ISupplementKitRecommendationService
{
    public Task<OperationResult<List<SupplementKitReportItem>>> GetSupplementRecommendations();
}