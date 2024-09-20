using System.Data;
using System.Net;
using ArticleAPI.Model;
using Dapper;
using Domain.Common.Core.Errors;
using Domain.Common.Core.Primitives;
using Domain.Common.Core.Primitives.Maybe;
using Domain.Core.Primitives.Result;
using Domain.Core.Utility;
using Domain.ValueObjects;
using Npgsql;
using Persistence.Infrastructure;

namespace ArticleAPI.Repositories;

internal sealed class ArticlesRepository(IHostEnvironment environment)
    : IArticlesRepository
{
    public async Task<Result> UpdateArticleAsync(ModifiedString description, ModifiedString title, Ulid articleId)
    {
        using NpgsqlConnection connection = DbConnection.CreateConnection(environment);
        await connection.OpenAsync();

        NpgsqlTransaction transaction = await connection.BeginTransactionAsync(IsolationLevel.Serializable);

        try
        {
            string? sql = $"""
                             UPDATE dbo.articles
                             SET title = @Title,
                                 description = @Description,
                                 modified_at = @ModifiedAt
                             WHERE id = @Id
                           """;

            var parameters = new
            {
                Id = articleId,
                Title = title.Value,
                Description = description.Value,
                ModifiedAt = DateTime.UtcNow
            };

            int result = await connection.ExecuteAsync(sql, parameters);
            Ensure.NotZero(result, "Result is 0. No rows were affected.", nameof(result));
        
            await transaction.CommitAsync();
        
            return await Result.Success();
        }
        catch (Exception exception)
        {
            await transaction.RollbackAsync();
            return await Result.Failure(new Error(HttpStatusCode.InternalServerError.ToString(), exception.Message));
        }
    }
    
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
                AuthorName = article.AuthorName,
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

    public async Task<Maybe<IEnumerable<Article>>> GetAllArticles()
    {
        await using NpgsqlConnection connection = DbConnection.CreateConnection(environment);
        await connection.OpenAsync();
        
        NpgsqlTransaction transaction = await connection.BeginTransactionAsync(IsolationLevel.Serializable);

        try
        {
            string? sql = $"""
                           SELECT * FROM dbo.articles
                           """;
            

            IEnumerable<Article>? result = await connection.QueryAsync<Article>(sql);
            
            await transaction.CommitAsync();

            return Maybe<IEnumerable<Article>>.From(result);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return Maybe<Maybe<IEnumerable<Article>>>.None;
        }
    }

    public async Task<Maybe<Article>> GetById(Ulid articleId)
    {
        await using NpgsqlConnection connection = DbConnection.CreateConnection(environment);
        await connection.OpenAsync();
        
        NpgsqlTransaction transaction = await connection.BeginTransactionAsync(IsolationLevel.Serializable);

        try
        {
            string? sql = $"""
                           SELECT * FROM dbo.articles WHERE id = @Id
                           """;

            var parameters = new
            {
                Id = articleId
            };

            Article? result = await connection.QueryFirstOrDefaultAsync<Article>(sql, parameters);
            
            await transaction.CommitAsync();

            return result!;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return Maybe<Article>.None;
        }
    }

    public async Task<Result> Delete(Ulid articleId)
    {
        await using NpgsqlConnection connection = DbConnection.CreateConnection(environment);
        await connection.OpenAsync();
        
        NpgsqlTransaction transaction = await connection.BeginTransactionAsync(IsolationLevel.Serializable);

        try
        {
            string? sql = $"""
                             DELETE FROM dbo.articles WHERE id = @Id
                           """;

            var parameters = new
            {
                Id = articleId
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