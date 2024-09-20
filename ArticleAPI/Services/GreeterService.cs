using Grpc.Core;
using ArticleAPI;
using ArticleAPI.Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Components;

namespace ArticleAPI.Services;

public class GreeterService : Greeter.GreeterBase
{
    private readonly ILogger<GreeterService> _logger;
    private readonly ISender _sender;

    public GreeterService(ILogger<GreeterService> logger, ISender sender)
    {
        _sender = sender;
        _logger = logger;
    }

    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        return Task.FromResult(new HelloReply
        {
            Message = "Hello " + request.Name
        });
    }
}