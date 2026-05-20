namespace GEE_Calculator.Domain.ActivityEntries;

public sealed class ActivityEntryException(string message) : InvalidOperationException(message);
