namespace ArticleAPI.Contract.Create;

public sealed record CreateRequest(
    string Title,
    string Description,
    string Picture_Link,
    string Content);