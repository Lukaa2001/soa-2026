using Explorer.BuildingBlocks.Core.UseCases;
using Explorer.Encounters.API.Dtos;
using Explorer.Encounters.API.Internal;
using Explorer.Payments.API.Dtos;
using Explorer.Payments.API.Internal;
using Explorer.Stakeholders.API.Dtos;
using Explorer.Stakeholders.API.Internal;
using FluentResults;

namespace Explorer.Tours.Service.Stubs;

// In the monolith these interfaces were satisfied by other modules in-process.
// In the Tours microservice the out-of-scope ones are stubbed; the in-scope
// cross-service calls (Tours -> Purchase) are wired over gRPC in Faza 7.

public class OrderServiceStub : IInternalOrderService
{
    public Result CreateOrder(long userId, List<long> tourId, string email) => Result.Ok();
    public Result<PagedResult<OrderDto>> GetByUser(int page, int pageSize, long userId)
        => Result.Ok(new PagedResult<OrderDto>(new List<OrderDto>(), 0));
    public Result<int> GetOrderCount(long tourId) => Result.Ok(0);
}

public class ShoppingCartServiceStub : IInternalShoppingCartService
{
    // Return an empty (non-null) item list so callers can safely enumerate Items.
    public Result<ShoppingCartDto> Create(long id) => Result.Ok(new ShoppingCartDto(id, new List<OrderItemDto>()));
    public Result<ShoppingCartDto> GetCartByUserId(long id) => Result.Ok(new ShoppingCartDto(id, new List<OrderItemDto>()));
}

public class ShoppingSessionStub : IInternalShoppingSession
{
    public void OpenShoppingSession(long id) { }
    public bool CheckActiveShoppingSession(long id) => false;
}

public class PersonServiceStub : IInternalPersonService
{
    public Result<string> GetName(long personId) => Result.Ok(string.Empty);
    public Result<string> GetSurname(long personId) => Result.Ok(string.Empty);
    public Result<StakeholdersCoordinateDto> GetPersonLocation(long personId)
        => Result.Ok(new StakeholdersCoordinateDto());
}

public class FollowersServiceStub : IInternalFollowersService
{
    public List<long> GetUsersFollowedIds(long userId) => new List<long>();
}

public class EncounterServiceStub : IInternalEncounterService
{
    public Result<EncounterDto> Create(EncounterDto encounter) => Result.Ok(encounter);
    // Encounters are out of scope: report "no encounter for this checkpoint" so the
    // execution's HasCompletedEncounter() returns true and checkpoints can complete.
    public Result<EncounterDto> GetForCheckpoint(long checkpointId) => Result.Fail("no encounter");
    public Result<EncounterDto> Update(EncounterDto encounter) => Result.Ok(encounter);
}

public class EncounterExecutionServiceStub : IInternalEncounterExecutionService
{
    public Result<EncounterExecutionDto> Activate(long encounterId, long touristId, EncounterCoordinateDto currentPosition)
        => Result.Ok(new EncounterExecutionDto());
    public Result<EncounterExecutionDto> Abandon(long executionId, long touristId)
        => Result.Ok(new EncounterExecutionDto());
    public Result<EncounterExecutionDto> CheckIfCompleted(long executionId, long touristId, EncounterCoordinateDto currentPosition)
        => Result.Ok(new EncounterExecutionDto());
    public Result<List<EncounterExecutionDto>> GetByEncounterId(long encounterId)
        => Result.Ok(new List<EncounterExecutionDto>());
}
