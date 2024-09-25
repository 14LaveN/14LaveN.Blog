using Application.ApiHelpers.Contracts;
using Application.ApiHelpers.Policy;
using Application.Core.Abstractions.Endpoint;
using Application.Core.Abstractions.Messaging;
using ArticleAPI.Model;
using Domain.Common.Core.Errors;
using Domain.Common.Core.Primitives.Maybe;
using Domain.Core.Exceptions;
using MediatR;

namespace ArticleAPI.Application.Queries;

public static class GetAllArticles
{
    public sealed record Query(TimeSpan? Expiration, string Key) : ICachedQuery<Maybe<IEnumerable<ArticleDto>>>;

    public sealed class GetAllArticlesEndpoint
        : IEndpoint
    {
        /// <inheritdoc />
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("api/v{version:apiVersion}/art", async (
                    ISender sender) =>
                {
                    var result = await Maybe<Query>
                        .From(new Query(TimeSpan.FromHours(1), $"get-articles-{DateTime.UtcNow}"))
                        .Bind(async query => await BaseRetryPolicy.Policy.Execute(async () =>
                            await sender.Send(query)));

                    return result.Value;
                })
                .AllowAnonymous()
                .Produces(StatusCodes.Status401Unauthorized, typeof(ApiErrorResponse))
                .Produces(StatusCodes.Status200OK)
                .RequireRateLimiting("fixed");
        }
    }
    
    internal sealed class QueryHandler(ArticleServices articleServices)
        : IQueryHandler<Query, Maybe<IEnumerable<ArticleDto>>>
    {
        public async Task<Maybe<IEnumerable<ArticleDto>>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            var result = await articleServices.ArticlesRepository.GetAllArticles();

            if (result.HasNoValue)
            {
                articleServices.Logger.LogWarning(DomainErrors.Article.NotFound);
                throw new NotFoundException(DomainErrors.Article.NotFound, nameof(DomainErrors.Article.NotFound));
            }

            return Maybe<IEnumerable<ArticleDto>>.From(result.Value
                .OrderBy(a => a.Created_At)
                .Select(x => 
                new ArticleDto(
                    x.Id, 
                    x.Description,
                    x.Title, 
                    x.Created_At,
                    x.Picture_Link is not null ? x.Picture_Link : "/pictureRest.jpg",
                    x.Content  is not null ? x.Content : "dfs")));
        }
    }
}