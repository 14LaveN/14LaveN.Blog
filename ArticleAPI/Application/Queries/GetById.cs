using Application.ApiHelpers.Contracts;
using Application.ApiHelpers.Policy;
using Application.Core.Abstractions.Endpoint;
using Application.Core.Abstractions.Messaging;
using ArticleAPI.Model;
using Domain.Common.Core.Errors;
using Domain.Common.Core.Primitives.Maybe;
using Domain.Core.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ArticleAPI.Application.Queries;

public static class GetById
{
    public sealed record Query(Ulid ArticleId)
        : ICommand<Maybe<ArticleDto>>;

    public sealed class GetArticleByIdEndpoint
        : IEndpoint
    {
        /// <inheritdoc />
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("api/v{version:apiVersion}/articles/{id}", async (
                    string id,
                    ISender sender) =>
                {
                    if (!Ulid.TryParse(id, out Ulid parsedId))
                        throw new UlidParseException(nameof(parsedId), "id");
                    
                    var result = await Maybe<Query>
                        .From(new Query(parsedId))
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
        : ICommandHandler<Query, Maybe<ArticleDto>>
    {
        public async Task<Maybe<ArticleDto>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            var result = await articleServices.ArticlesRepository.GetById(request.ArticleId);

            if (result.HasNoValue)
            {
                articleServices.Logger.LogWarning(DomainErrors.Article.NotFound);
                throw new NotFoundException(DomainErrors.Article.NotFound, nameof(DomainErrors.Article.NotFound));
            }

            return new ArticleDto(result.Value.Id, result.Value.Description.Value, result.Value.Title.Value, result.Value.Created_At, result.Value.Picture_Link.Value, result.Value.Content.Value);
        }
    }
}