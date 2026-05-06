namespace GEE_Calculator.Domain.Calculations;

public interface IEmissionCalculationService
{
    CalculateEmissionPreviewResponse Preview(CalculateEmissionPreviewRequest request);
    Task<RunEmissionCalculationResponse> RunAsync(RunEmissionCalculationRequest request, CancellationToken cancellationToken = default);
}
