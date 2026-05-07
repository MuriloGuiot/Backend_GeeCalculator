namespace GEE_Calculator.Domain.Common;

public sealed class TenantContextException(string message) : InvalidOperationException(message);
