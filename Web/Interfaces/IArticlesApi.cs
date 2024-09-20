using ArticleAPI;
using ArticleAPI.Model;
using Refit;

namespace Web.Interfaces;

public interface IArticlesApi
{
    [Get("/articles")]
    Task<IEnumerable<ArticleDtoObject>> GetArticlesAsync();
}