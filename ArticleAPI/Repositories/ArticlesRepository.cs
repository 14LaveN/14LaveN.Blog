using System.Data;
using System.Net;
using ArticleAPI.Model;
using Dapper;
using Domain.Common.Core.Errors;
using Domain.Common.Core.Primitives;
using Domain.Core.Primitives.Result;
using Domain.Core.Utility;
using Npgsql;
using Persistence.Infrastructure;

namespace ArticleAPI.Repositories;

internal sealed class ArticlesRepository(IHostEnvironment environment)
    : IArticlesRepository
{
    public async Task<Result> Create(Article article)
    {
        await using NpgsqlConnection connection = DbConnection.CreateConnection(environment);
        await connection.OpenAsync();
        
        NpgsqlTransaction transaction = await connection.BeginTransactionAsync(IsolationLevel.Serializable);

        try
        {
            string? sql = $"""
                        INSERT INTO dbo.articles (id, title, description, author_name, author_id, created_at, modified_at)
                        VALUES (@Id, @Title, @Description, @AuthorName, @AuthorId, @CreatedAt, @ModifiedAt) 
                      """;

            var parameters = new
            {
                Id = article.CreateId().Value,
                Title = article.Title.Value,
                Description = article.Description.Value,
                AuthorName = "fdgdfgdfg",
                AuthorId = article.CreateId().Value,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = (DateTime?)default
            };

            int result = await connection.ExecuteAsync(sql, parameters);
            Ensure.NotZero(result, "Result is 0.", nameof(result));
            
            await transaction.CommitAsync();
            
            return await Result.Success();
        }
        catch (Exception exception)
        {
            await transaction.RollbackAsync();
            return await Result.Failure(new Error(HttpStatusCode.InternalServerError.ToString(),exception.Message));
        }
    }
}