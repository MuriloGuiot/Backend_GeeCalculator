namespace GEE_Calculator.Domain.Calculations;

public interface IEmissionCalculationService
{
    CalculateEmissionPreviewResponse Preview(CalculateEmissionPreviewRequest request);
    Task<RunEmissionCalculationResponse> RunAsync(RunEmissionCalculationRequest request, CancellationToken cancellationToken = default);
    Task<RunEmissionCalculationResponse> CalculateInventoryAsync(
        Guid inventoryId,
        CalculateInventoryRequest request,
        CancellationToken cancellationToken = default);
}
