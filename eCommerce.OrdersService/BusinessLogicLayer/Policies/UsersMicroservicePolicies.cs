using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;


namespace BusinessLogicLayer.Policies;

public sealed class UsersMicroservicePolicies
{
    private readonly ILogger<UsersMicroservicePolicies> _logger;
    private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;
    private readonly IAsyncPolicy<HttpResponseMessage> _circuitBreaker;
    private readonly IAsyncPolicy<HttpResponseMessage> _timeoutPolicy;
    private readonly IAsyncPolicy<HttpResponseMessage> _allPolicies;
    
    public UsersMicroservicePolicies(ILogger<UsersMicroservicePolicies> logger, IOptions<UsersPoliciesSettings> config)
    {
        _logger = logger;
        var s = config.Value;

        _retryPolicy = Policy<HttpResponseMessage>
            .HandleResult(r => !r.IsSuccessStatusCode)
            .WaitAndRetryAsync(retryCount: s.RetryCount, sleepDurationProvider: retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetryAsync: (outcome, timeSpan, retryCount, context) =>
                {
                    _logger.LogInformation("Retrying request {RetryCount} to Users Microservice " +
                                           "after {TimeSpanTotalSeconds} seconds ", retryCount, timeSpan.TotalSeconds);
                    return Task.CompletedTask;
                });

        _circuitBreaker = Policy<HttpResponseMessage>
            .HandleResult(r => !r.IsSuccessStatusCode)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: s.AllowedFailures,
                durationOfBreak: TimeSpan.FromSeconds(s.BreakDurationSeconds),
                onBreak: (outcome, timeSpan) =>
                {
                    _logger.LogWarning("Circuit breaker opened for {TimeSpanTotalSeconds} seconds " +
                                       "due to {Reason}", timeSpan.TotalSeconds, outcome.Result.StatusCode);
                },
                onReset: () => _logger.LogInformation("Circuit breaker reset."));

        _timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromMilliseconds(s.TimeoutMs));

        _allPolicies = Policy.WrapAsync(
            _circuitBreaker, // outer
            _retryPolicy, // middle
            _timeoutPolicy // inner
        );
    }

    public IAsyncPolicy<HttpResponseMessage> Retry => _retryPolicy;
    public IAsyncPolicy<HttpResponseMessage> Breaker => _circuitBreaker;
    public IAsyncPolicy<HttpResponseMessage> Timeout => _timeoutPolicy;
    public IAsyncPolicy<HttpResponseMessage> AllPolicies => _allPolicies;
}