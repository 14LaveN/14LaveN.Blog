using ArticleAPI.Services;
using Microsoft.AspNetCore.Mvc;
using ArticlesGrpcService = ArticleAPI.Articles.ArticlesClient;

namespace ArticleAPI.Controllers;

public class ArticlesService(ArticlesGrpcService grpcClient)
{
    public async Task<IEnumerable<ArticleDtoObject>> GetArticles()
    {
        var request = new GetArticlesRequest();
        var response = await grpcClient.GetArticles(request);
        
        return response.Articles;
    }
}