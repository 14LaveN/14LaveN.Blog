using System.Net;
using Application.ApiHelpers.Responses;
using Application.Core.Abstractions.Messaging;
using ArticleAPI.Model;
using Domain.Common.Core.Errors;
using Domain.Common.Core.Exceptions;
using Domain.Common.Core.Primitives.Maybe;
using Domain.Core.Exceptions;
using Domain.Core.Primitives.Result;
using Google.Protobuf.WellKnownTypes;

namespace ArticleAPI.Application.Queries;

public static class GetAllArticles
{
    public sealed record Query
        : ICommand<Maybe<IEnumerable<ArticleDtoObject>>>;

    internal sealed class QueryHandler(ArticleServices articleServices)
        : ICommandHandler<Query, Maybe<IEnumerable<ArticleDtoObject>>>
    {
        public async Task<Maybe<IEnumerable<ArticleDtoObject>>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            var result = await articleServices.ArticlesRepository.GetAllArticles();

            if (result.HasNoValue)
            {
                articleServices.Logger.LogWarning(DomainErrors.Article.NotFound);
                throw new NotFoundException(DomainErrors.Article.NotFound, nameof(DomainErrors.Article.NotFound));
            }

            return Maybe<IEnumerable<ArticleDtoObject>>.From(result.Value.Select(x => new ArticleDtoObject
            {
                CreatedAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(x.CreatedAt.ToUniversalTime()),
                Description = x.Description,
                Title = x.Title
            }));
        }
    }
}