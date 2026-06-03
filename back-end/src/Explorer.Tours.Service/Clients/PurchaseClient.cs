using System.Net.Http.Json;

namespace Explorer.Tours.Service.Clients;

// Service-to-service call (Tours -> Purchase): verifies a tour was bought
// before a tour execution can be started (functionality 17 prerequisite).
public class PurchaseClient
{
    private readonly HttpClient _http;

    public PurchaseClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<bool> HasPurchased(long userId, long tourId)
    {
        try
        {
            return await _http.GetFromJsonAsync<bool>($"api/order/internal/has-purchased/{userId}/{tourId}");
        }
        catch
        {
            return false;
        }
    }
}
