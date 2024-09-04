using System.Security.Claims;
using Grpc.Core;

namespace ArticleAPI.Extensions;

internal static class ServerCallContextIdentityExtensions
{
    public static string? GetUserIdentity(this ServerCallContext context) => 
        context.GetHttpContext().User.FindFirst("nameid")?.Value;
    
    public static string? GetUserName(this ServerCallContext context) => 
        context.GetHttpContext().User.FindFirst("name")?.Value;
}
