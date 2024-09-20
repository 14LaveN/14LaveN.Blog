using Domain.Common.Core.Errors;
using Domain.Common.Core.Primitives;
using Domain.Common.Core.Primitives.Result;
using Domain.Core.Primitives.Result;

namespace Domain.ValueObjects;

/// <summary>
/// Represents the modified string value object.
/// </summary>
public sealed class ModifiedString : ValueObject
{
    /// <summary>
    /// The name maximum length.
    /// </summary>
    public const int MaxLength = 512;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModifiedString"/> class.
    /// </summary>
    /// <param name="value">The name value.</param>
    public ModifiedString(string value) => Value = value;

    /// <summary>
    /// Gets the name value.
    /// </summary>
    public string Value { get; set; }

    public static implicit operator string(ModifiedString name) =>
        name.Value;

    public static implicit operator ModifiedString(string name) =>
        new(name);
    
    public Ulid ProductId { get; set; }
    
    /// <summary>
    /// Creates a new <see cref="ModifiedString"/> instance based on the specified value.
    /// </summary>
    /// <param name="modifiedString">The modified string value.</param>
    /// <returns>The result of the name creation process containing the name or an error.</returns>
    public static Result<ModifiedString> Create(string modifiedString) =>
        Result.Create(modifiedString, DomainErrors.Name.NullOrEmpty)
            .Ensure(n => !string.IsNullOrWhiteSpace(n), DomainErrors.Name.NullOrEmpty)
            .Ensure(n => n.Length <= MaxLength, DomainErrors.Name.LongerThanAllowed)
            .Map(f => new ModifiedString(f));

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <inheritdoc />
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}