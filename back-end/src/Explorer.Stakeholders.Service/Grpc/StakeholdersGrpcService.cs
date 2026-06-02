using Explorer.Grpc.Stakeholders;
using Explorer.Stakeholders.API.Internal;
using Explorer.Stakeholders.API.Public;
using Grpc.Core;

namespace Explorer.Stakeholders.Service.Grpc;

// gRPC server implementation. Backed by the same use cases the REST controllers use.
public class StakeholdersGrpcService : StakeholdersGrpc.StakeholdersGrpcBase
{
    private readonly IInternalUserService _userService;
    private readonly IPersonService _personService;

    public StakeholdersGrpcService(IInternalUserService userService, IPersonService personService)
    {
        _userService = userService;
        _personService = personService;
    }

    public override Task<UserReply> GetUser(UserRequest request, ServerCallContext context)
    {
        var result = _userService.GetUser(request.UserId);
        if (result.IsFailed)
            throw new RpcException(new Status(StatusCode.NotFound, $"User {request.UserId} not found"));

        return Task.FromResult(new UserReply
        {
            Id = result.Value.Id,
            Username = result.Value.Username ?? string.Empty
        });
    }

    public override Task<EmailReply> GetPersonEmail(PersonRequest request, ServerCallContext context)
    {
        var result = _personService.GetPersonEmail(request.PersonId);
        return Task.FromResult(new EmailReply
        {
            Email = result.IsSuccess ? result.Value ?? string.Empty : string.Empty
        });
    }
}
