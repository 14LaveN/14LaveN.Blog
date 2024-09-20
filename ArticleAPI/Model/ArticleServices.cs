using ArticleAPI.Repositories;

namespace ArticleAPI.Model;

public sealed class ArticleServices(
    ILogger<ArticleServices> logger,
    IArticlesRepository articlesRepository)
{
    public readonly ILogger<ArticleServices> Logger = logger;

    public readonly IArticlesRepository ArticlesRepository = articlesRepository;
}