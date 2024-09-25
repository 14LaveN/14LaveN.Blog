using System.Text.Json.Serialization;

namespace ArticleAPI.Model;

public sealed record ArticleDto(
    [property: JsonPropertyName("articleId")] string ArticleId,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("Created_At")] DateTime Created_At,
    [property: JsonPropertyName("Picture_Link")] string Picture_Link,
    [property: JsonPropertyName("content")] string Content);