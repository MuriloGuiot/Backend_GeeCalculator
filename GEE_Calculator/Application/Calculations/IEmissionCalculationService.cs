namespace GEE_Calculator.Application.Calculations;

public interface IEmissionCalculationService
{
    CalculateEmissionPreviewResponse Preview(CalculateEmissionPreviewRequest request);
    Task<RunEmissionCalculationResponse> RunAsync(RunEmissionCalculationRequest request, CancellationToken cancellationToken = default);
}
