using System.Data;
using ArticleAPI.Model;
using Dapper;
using Domain.Common.Core.Primitives.Maybe;
using Domain.Core.Primitives.Result;
using Domain.ValueObjects;
using Persistence.Infrastructure;

namespace ArticleAPI.Repositories;

public interface IArticlesRepository
{
    Task<Result> UpdateArticleAsync(ModifiedString description, ModifiedString title, Ulid articleId);
    
    Task<Result> Create(Article article);

    Task<Maybe<Article>> GetById(Ulid articleId);

    Task<Result> Delete(Ulid articleId);
}