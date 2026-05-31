using Microsoft.AspNetCore.Http;

namespace Explorer.API.Controllers;

public static class HttpContextExtensions
{
    /// <summary>
    /// Reads the personId claim from the JWT carried on the request.
    /// </summary>
    public static long GetPersonId(this HttpContext source)
    {
        try
        {
            return long.Parse(source?.User?.Claims?.FirstOrDefault(c => c.Type == "personId")?.Value!);
        }
        catch (Exception e)
        {
            throw new Exception("Person id is not valid. " + e.Message);
        }
    }
}
