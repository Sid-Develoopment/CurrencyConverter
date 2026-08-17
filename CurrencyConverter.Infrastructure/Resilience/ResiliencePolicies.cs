namespace CurrencyConverter.Infrastructure.Resilience;

public static class ResiliencePolicies
{
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int maxRetries = 3) =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(maxRetries, retry => 
                TimeSpan.FromSeconds(Math.Pow(2, retry)) + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 100)));

    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(
        int handledEventsAllowedBeforeBreaking = 5, 
        TimeSpan durationOfBreak = default) =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(handledEventsAllowedBeforeBreaking, durationOfBreak == default ? TimeSpan.FromSeconds(30) : durationOfBreak);
}