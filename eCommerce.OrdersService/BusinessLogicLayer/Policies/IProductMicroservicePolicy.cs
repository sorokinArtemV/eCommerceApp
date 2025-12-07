using Polly;

namespace BusinessLogicLayer.Policies;

public interface IProductMicroservicePolicy
{
    public IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy();
    public IAsyncPolicy<HttpResponseMessage> GetBulkheadPolicy();
}