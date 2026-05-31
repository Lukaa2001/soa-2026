using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Mime;

namespace Explorer.API.Middleware.ExceptionModel;

public static class Utils
{
    public static HttpStatusCode ExceptionToStatus(this Exception exception)
    {
        switch (exception)
        {
            case ArgumentException: return HttpStatusCode.BadRequest;
            case KeyNotFoundException: return HttpStatusCode.NotFound;
            default: return HttpStatusCode.InternalServerError;
        }
    }

    public static async Task WriteJsonToHttpResponseAsync<TResponse>(HttpResponse httpResponse, HttpStatusCode statusCode, TResponse response)
    {
        httpResponse.ContentType = MediaTypeNames.Application.Json;
        httpResponse.StatusCode = (int)statusCode;
        await httpResponse.WriteAsJsonAsync(response);
    }
}
