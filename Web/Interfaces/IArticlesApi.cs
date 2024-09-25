using ArticleAPI;
using ArticleAPI.Model;
using Domain.Common.Core.Primitives.Maybe;
using Microsoft.AspNetCore.Mvc;
using Refit;

namespace Web.Interfaces;

public interface IArticlesApi
{
    [Get("/api/v1/articles")]
    Task<IEnumerable<string>> GetArticlesAsync();
    
    [Get("/api/v1/articles/{id}")]
    Task<ArticleDto> GetArticleById([FromQuery] string id);
}