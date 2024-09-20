using System.Net;
using Application.ApiHelpers.Responses;
using Application.Core.Abstractions.Messaging;
using ArticleAPI.Model;
using ArticleAPI.Repositories;
using Domain.Common.Core.Errors;
using Domain.Common.Core.Exceptions;
using Domain.Core.Primitives.Result;
using Domain.ValueObjects;

namespace ArticleAPI.Application.Commands;

public static class Create
{
    public sealed record Command(
        ModifiedString Title,
        ModifiedString Description)
        : ICommand<IBaseResponse<Result>>;

    internal sealed class CommandHandler(ArticleServices articleServices)
        : ICommandHandler<Command, IBaseResponse<Result>>
    {
        public async Task<IBaseResponse<Result>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            Result<Article> articleResult = Article.Create(request.Title, request.Description, Ulid.NewUlid());

            Article article = articleResult.IsSuccess 
                ? articleResult.Value 
                : throw new DomainException(DomainErrors.Article.HasAlreadyTaken);

            Result result = await articleServices.ArticlesRepository.Create(article);

            return new BaseResponse<Result>
            {
                StatusCode = HttpStatusCode.OK,
                Description = "Article created.",
                Data = result
            };
        }
    }
}