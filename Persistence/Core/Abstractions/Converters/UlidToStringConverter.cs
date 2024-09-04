using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Persistence.Core.Abstractions.Converters;

public sealed class UlidToStringConverter() : ValueConverter<Ulid, string>(
    ulid => ulid.ToString(),
    str => Ulid.Parse(str));