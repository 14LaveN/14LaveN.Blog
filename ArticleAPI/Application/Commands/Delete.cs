using System.Net;
using Application.ApiHelpers.Responses;
using Application.Core.Abstractions.Messaging;
using Application.Core.Errors;
using Application.Core.Extensions;
using ArticleAPI.Model;
using Domain.Common.Core.Errors;
using Domain.Common.Core.Primitives.Maybe;
using Domain.Core.Exceptions;
using Domain.Core.Primitives.Result;
using Domain.ValueObjects;
using FluentValidation;

namespace ArticleAPI.Application.Commands;

public static class Delete
{
    public sealed record Command(Ulid ArticleId)
        : ICommand<IBaseResponse<Result>>;

    internal sealed class CommandHandler(ArticleServices articleServices)
        : ICommandHandler<Command, IBaseResponse<Result>>
    {
        public async Task<IBaseResponse<Result>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            Maybe<Article> article = await articleServices.ArticlesRepository.GetById(request.ArticleId);

            if (article.HasNoValue)
            {
                articleServices.Logger.LogWarning(DomainErrors.Article.NotFound);
                throw new NotFoundException(DomainErrors.Article.NotFound, nameof(DomainErrors.Article.NotFound));
            }
            
            Result articleResult = await articleServices.ArticlesRepository.Delete(request.ArticleId);

            return new BaseResponse<Result>
            {
                StatusCode = HttpStatusCode.OK,
                Description = "Article created.",
                Data = articleResult
            };
        }
    }
    internal class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.ArticleId.ToString())
                .NotEqual(Ulid.Empty.ToString())
                .WithMessage("You don't write the identifier.");
        }
    }
}