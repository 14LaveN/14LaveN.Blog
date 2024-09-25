using System.Net;
using Application.ApiHelpers.Contracts;
using Application.ApiHelpers.Policy;
using Application.ApiHelpers.Responses;
using Application.Core.Abstractions.Endpoint;
using Application.Core.Abstractions.Messaging;
using Application.Core.Errors;
using Application.Core.Extensions;
using ArticleAPI.Model;
using ArticleAPI.Repositories;
using Domain.Common.Core.Errors;
using Domain.Common.Core.Exceptions;
using Domain.Common.Core.Primitives.Result;
using Domain.Core.Primitives.Result;
using Domain.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace ArticleAPI.Application.Commands;

public static class Create
{
    public sealed record Command(
        ModifiedString Title,
        ModifiedString Description,
        ModifiedString Picture_Link,
        ModifiedString Content)
        : ICommand<IBaseResponse<Result>>;
    
    public sealed class CreateEndpoint
        : IEndpoint
    {
        /// <inheritdoc />
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/v{version:apiVersion}/article", async (
                    [FromBody] Contract.Create.CreateRequest request,
                    ISender sender) =>
                {
                    var result = await Result.Create(request, DomainErrors.General.UnProcessableRequest)
                        .Map(createRequest => new Command(createRequest.Title, createRequest.Description, createRequest.Picture_Link, createRequest.Content))
                        .Bind(command => Task.FromResult(BaseRetryPolicy.Policy.Execute(async () =>
                            await sender.Send(command)).Result.Data));

                    return result;
                })
                .AllowAnonymous()
                .Produces(StatusCodes.Status401Unauthorized, typeof(ApiErrorResponse))
                .Produces(StatusCodes.Status200OK)
                .RequireRateLimiting("fixed");
        }
    }

    internal sealed class CommandHandler(ArticleServices articleServices)
        : ICommandHandler<Command, IBaseResponse<Result>>
    {
        public async Task<IBaseResponse<Result>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            Result<Article> articleResult = Article.Create(request.Title, request.Description, request.Picture_Link,request.Content, Ulid.NewUlid());
            
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

    internal class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Title.Value)
                .NotEqual(string.Empty)
                .MaximumLength(256)
                .WithError(ValidationErrors.CreatePost.TitleIsRequired);
            
            RuleFor(c => c.Description.Value)
                .NotEqual(string.Empty)
                .MaximumLength(512)
                .WithError(ValidationErrors.CreatePost.DescriptionIsRequired);
        }
    }
}