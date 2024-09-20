using ArticleAPI.Application.Commands;
using ArticleAPI.Application.Queries;
using Grpc.Core;
using MediatR;

namespace ArticleAPI.Services;

public class ArticlesService : Articles.ArticlesClient
{
    private readonly ISender _sender;

    public ArticlesService(ISender sender)
    {
        _sender = sender;
    }

    public override async Task<GetArticlesResponse> GetArticles(GetArticlesRequest request, CallOptions options)
    {
        var result = await _sender.Send(new GetAllArticles.Query());
        
        var response = new GetArticlesResponse();
        
        response.Articles.AddRange(result.Value);

        return response;
    }

    public override async Task<CreateResponse> CreateArticle(CreateRequest request, CallOptions options)
    {
        var result = await _sender.Send(new Create.Command(request.Title, request.Description));

        return new CreateResponse();
    }
}