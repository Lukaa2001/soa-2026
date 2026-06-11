using Explorer.BuildingBlocks.Core.UseCases;
using Explorer.Stakeholders.API.Dtos;
using Explorer.Stakeholders.API.Public.Administration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Explorer.API.Controllers;

// Functionality 9 support: any authenticated user can list all users so they can
// choose whom to follow (not just the graph-based suggestions).
[Authorize]
[Route("api/users")]
public class UsersController : BaseApiController
{
    private readonly IUserManagmentService _userManagmentService;

    public UsersController(IUserManagmentService userManagmentService)
    {
        _userManagmentService = userManagmentService;
    }

    [HttpGet("all")]
    public ActionResult<PagedResult<UserDto>> GetAll()
    {
        // page/pageSize 0 returns the whole set.
        var result = _userManagmentService.GetPaged(0, 0);
        return CreateResponse(result);
    }
}
