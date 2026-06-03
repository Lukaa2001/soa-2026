using System.Net.Http.Json;

namespace Explorer.Blog.Service.Clients;

// Service-to-service call (Blog -> Followers): a user may comment on a blog only
// if they follow its author (functionality 9).
public class FollowersClient
{
    private readonly HttpClient _http;

    public FollowersClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<bool> IsFollowing(long followerId, long followedId)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<IsFollowingResponse>(
                $"api/followers/users/isFollowing/{followerId}/{followedId}");
            return response?.IsFollowing ?? false;
        }
        catch
        {
            return false;
        }
    }

    private sealed record IsFollowingResponse(bool IsFollowing);
}
