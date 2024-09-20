using Microsoft.EntityFrameworkCore;
using Persistence;
using Application.Core.Abstractions;
using Application.Core.Abstractions.Idempotency;

namespace Persistence.Idempotency;

/// <summary>
/// Represents the idempotency service class.
/// </summary>
/// <param name="dbContext">The database context.</param>
internal sealed class IdempotencyService(BaseDbContext dbContext) : IIdempotencyService
{
    /// <inheritdoc />
    public async Task<bool> RequestExistsAsync(Ulid requestId) =>
         await dbContext
             .Set<IdempotentRequest>()
             .AnyAsync(r => r.Id == requestId);
    

    /// <inheritdoc />
    public async Task CreateRequestAsync(Ulid requestId, string name)
    {
        IdempotentRequest idempotentRequest = new IdempotentRequest
        {
            Id = requestId,
            Name = name,
            CreatedOnUtc = DateTime.UtcNow
        };

        await dbContext
            .Set<IdempotentRequest>()
            .AddAsync(idempotentRequest);
        
        await dbContext.SaveChangesAsync();
    }
}