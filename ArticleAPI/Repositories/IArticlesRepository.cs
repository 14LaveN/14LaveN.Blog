using System.Data;
using ArticleAPI.Model;
using Dapper;
using Domain.Core.Primitives.Result;
using Persistence.Infrastructure;

namespace ArticleAPI.Repositories;

public interface IArticlesRepository
{
    Task<Result> Create(Article article);
}