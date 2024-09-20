using ArticleAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ArticleAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class ArticlesController : ControllerBase
{
    private readonly Articles.ArticlesClient _grpcClient;

    public ArticlesController(ArticlesService grpcClient)
    {
        _grpcClient = grpcClient;
    }

    [HttpGet]
    public async Task<IEnumerable<ArticleDtoObject>> GetArticles()
    {
        var request = new GetArticlesRequest();
        var response = await _grpcClient.GetArticles(request);
        
        return response.Articles;
    }
}