using Explorer.Grpc.Stakeholders;
using Explorer.Stakeholders.API.Dtos;
using Explorer.Stakeholders.API.Internal;
using FluentResults;

namespace Explorer.Blog.Service.Stubs;

// Service -> service RPC: the Blog service resolves author/commenter usernames
// by calling the Stakeholders gRPC server.
public class GrpcUserService : IInternalUserService
{
    private readonly StakeholdersGrpc.StakeholdersGrpcClient _client;

    public GrpcUserService(StakeholdersGrpc.StakeholdersGrpcClient client)
    {
        _client = client;
    }

    public Result<UserDto> GetUser(long userId)
    {
        try
        {
            var reply = _client.GetUser(new UserRequest { UserId = userId });
            return Result.Ok(new UserDto { Id = reply.Id, Username = reply.Username });
        }
        catch
        {
            // Keep blog responses well-formed even if Stakeholders is unavailable.
            return Result.Ok(new UserDto { Id = userId, Username = $"user{userId}" });
        }
    }
}
