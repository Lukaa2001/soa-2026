using System.Security.Claims;

namespace Explorer.API.Controllers;

public static class ClaimsPrincipalExtensions
{
    public static int PersonId(this ClaimsPrincipal user)
        => int.Parse(user.Claims.First(i => i.Type == "personId").Value);

    public static int UserId(this ClaimsPrincipal user)
        => int.Parse(user.Claims.First(i => i.Type == "id").Value);
}
