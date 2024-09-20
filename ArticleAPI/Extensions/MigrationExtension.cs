using Npgsql;
using Persistence.Infrastructure;

namespace ArticleAPI.Extensions;

public static class MigrationExtension
{
    //TODO 
    
    public static async Task<IServiceCollection> ApplyMigrations(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        IServiceCollection services = builder.Services;
        IHostEnvironment environment = builder.Environment;
        
        await using NpgsqlConnection connection = DbConnection.CreateConnection(environment);

        await connection.OpenAsync();

        await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();

        try
        {
            string dboSql = $"CREATE SCHEMA IF NOT EXISTS dbo;";
            
            string sql = $"""
                            CREATE TABLE dbo.articles(
                         	id TEXT PRIMARY KEY NOT NULL,
                         	title TEXT NOT NULL,             
                             description TEXT NOT NULL,       
                             author_name TEXT NOT NULL,        
                             author_id TEXT NOT NULL,          
                             created_at TIMESTAMP NOT NULL,    
                             modified_at TIMESTAMP DEFAULT NULL
                         );

                         CREATE UNIQUE INDEX idx_id ON dbo.articles (id);
                         
                         CLUSTER dbo.articles USING idx_id;
                         
                         CREATE INDEX a_idx_aid ON dbo.articles USING HASH (author_id);
                         """;
            
            

            await transaction.CommitAsync();

            //SELECT master_add_node('worker1_ip', 5433);
            //SELECT master_add_node('worker2_ip', 5433);
            //SELECT create_distributed_table('articles', 'author_id'); -- In Linux server or Docker.

        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
        }
        
        return services;
    }
}