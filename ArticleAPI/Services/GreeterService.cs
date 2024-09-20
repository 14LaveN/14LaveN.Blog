using Grpc.Core;
using ArticleAPI;
using ArticleAPI.Application.Commands;
using MediatR;

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

    public override async Task<CreateResponse> CreateArticle(CreateRequest request, ServerCallContext context)
    {
        var result = await _sender.Send(new Create.Command(request.Title, request.Description));

        return new CreateResponse();
    }
    
}