using ArticleAPI.Application.Commands;
using Grpc.Core;
using MediatR;

namespace ArticleAPI.Services;

public class ArticlesService : Articles.ArticlesBase
{
    private readonly ISender _sender;

    public ArticlesService(ISender sender)
    {
        _sender = sender;
    }

    public override async Task<CreateResponse> CreateArticle(CreateRequest request, ServerCallContext context)
    {
        var result = await _sender.Send(new Create.Command(request.Title, request.Description));

        return new CreateResponse();
    }
    
}