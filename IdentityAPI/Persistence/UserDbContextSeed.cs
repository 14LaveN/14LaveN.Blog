using System.Numerics;
using Domain.Common.ValueObjects;
using Domain.ValueObjects;
using Identity.API.Common;
using Identity.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Persistence;
using Persistence.Core.Extensions;

namespace Identity.API.Persistence;

public sealed class UserDbContextSeed(
    ILogger<UserDbContextSeed> logger) 
    : IDbSeeder<UserDbContext>
{
    public async Task SeedAsync(UserDbContext context)
    {
        await context.Database.OpenConnectionAsync();
        await ((NpgsqlConnection)context.Database.GetDbConnection()).ReloadTypesAsync();

        if (!context.Set<User>().Any())
        {
            context.Set<User>().RemoveRange(context.Set<User>());
            //TODO await context.Set<User>().AddRangeAsync([
            //TODO     new User(
            //TODO     "hfdgdgg",
            //TODO     FirstName.Create("dfdffsdfdsf").Value,
            //TODO     LastName.Create("dfdsfdsfsd").Value,
            //TODO     EmailAddress.Create("sasha@mail.ru").Value,
            //TODO     "Sasha_2008!"),
            //TODO     new User(
            //TODO         "hfdfdgdfg",
            //TODO         FirstName.Create("dfdffsdfdgdfgf").Value,
            //TODO         LastName.Create("dfdfgfdgdsfsd").Value,
            //TODO         EmailAddress.Create("sasha@mail.ru").Value,
            //TODO         "Sasha_2008!"),
            //TODO     new User(
            //TODO         "hdfgfdgfdgdfg",
            //TODO         FirstName.Create("dfdfdgdfgf").Value,
            //TODO         LastName.Create("dfdfgfdgdsfsd").Value,
            //TODO         EmailAddress.Create("sasha@mail.ru").Value,
            //TODO         "Sasha_2008!")
            //TODO ]);
            logger.LogInformation(
                "Seeded users with {NumBrands}.", 
                context.Set<User>().Count());
            
            await context.SaveChangesAsync();
        }
    }
}
