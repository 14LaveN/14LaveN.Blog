using Domain.Common.Core.Errors;
using Domain.Common.Core.Primitives;
using Domain.Core.Primitives;
using Domain.Core.Primitives.Result;
using Domain.Core.Utility;
using stringMod = Domain.ValueObjects.ModifiedString;

namespace ArticleAPI.Model;

public sealed class Article
    : Entity
{
    #region Constructors

    private Article(
        stringMod title,
        stringMod description,
        Ulid authorId)
    {
        Ensure.NotEmpty(title, "Title is required.", nameof(title));
        Ensure.NotEmpty(description, "Description is required.", nameof(description));
        Ensure.NotEmpty(authorId, "Author identifier is required.", nameof(authorId));
        
        Title = title;
        AuthorId = authorId;
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }

    public Article()
    {
    }

    #endregion

    public static Result<Article> Create(
        stringMod title,
        stringMod description,
        Ulid authorId)
    {
        if (authorId == Ulid.Empty)
            return Result.Failure<Article>(DomainErrors.Ulid.IsEmpty);
        
        Article article = new Article(title, description, authorId);

        return Result.Success(article);
    }

    #region Changing methods.

    public Result<Article> ChangeTitle(stringMod title)
    {
        if (Title == title)
            return Result.Failure<Article>(DomainErrors.Article.CannotChangeTitle);

        Title = title;
        ModifiedAt = DateTime.UtcNow;

        return Result.Success(this);
    }
    
    public Result<Article> ChangeDescription(stringMod description)
    {
        if (Description == description)
            return Result.Failure<Article>(DomainErrors.Article.CannotChangeDescription);

        Description = description;
        ModifiedAt = DateTime.UtcNow;

        return Result.Success(this);
    }

    #endregion

    #region Properties.

    public stringMod Title { get; private set; }
    public stringMod Description { get; private set; }

    public stringMod AuthorName { get; private set; } = "fdsfdsfsdf";
    public Ulid AuthorId { get; private set; }

    public DateTime CreatedAt { get; init; }

    public DateTime ModifiedAt { get; private set; } = default;

    #endregion
    
}