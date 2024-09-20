using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Persistence.Core.Abstractions.Connections;

namespace Persistence.Infrastructure;

/// <summary>
/// Represents the db connection static class.
/// </summary>
public static class DbConnection
{
    private const string? DevConnectionString = "Server=localhost;Port=5433;Database=BGenericDb;Username=postgres;Password=1111;";
    private const string? ProdConnectionString = "Server=localhost;Port=5432;Database=BGenericDb;Username=sasha;Password=1111;";
    
    /// <summary>
    /// Create db connection with specified connection string.
    /// </summary>
    /// <returns>Returns db connection.</returns>
    public static NpgsqlConnection CreateConnection(
        IHostEnvironment environment)
    {
        return environment.EnvironmentName switch
        {
            EnvironmentConstants.DevelopmentEnvironment =>
                new NpgsqlConnection(DevConnectionString),
            
            EnvironmentConstants.ProductionEnvironment => 
                new NpgsqlConnection(ProdConnectionString),
            
            _ => throw new ConnectionAbortedException()
        };
    }
}