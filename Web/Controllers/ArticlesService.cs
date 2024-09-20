using ArticleAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ArticleAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class ArticlesController(Articles.ArticlesClient articlesClient) : Controller
{
    [HttpGet]
    public async Task<IEnumerable<ArticleDtoObject>> GetArticles()
    {
        var request = new GetArticlesRequest();
        var response = await articlesClient.GetArticlesAsync(request);
        
        return response.Articles;
    }
}