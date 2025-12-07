namespace BusinessLogicLayer.Policies;

public sealed class UsersPoliciesSettings
{
    public int RetryCount { get; init; }
    public int RetryBaseDelayMs { get; init; }

    public int AllowedFailures { get; init; }
    public int BreakDurationSeconds { get; init; }

    public int TimeoutMs { get; init; }
}