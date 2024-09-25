using Application.Core.Behaviours;
using ArticleAPI.Application.Commands;
using ArticleAPI.Application.Queries;
using FluentValidation;
using MediatR.NotificationPublishers;

namespace ArticleAPI.Extensions;

public static class MediatrExtensions
{
    public static IServiceCollection AddMediatr(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        
        services.AddValidatorsFromAssemblyContaining<Program>();
        
        services.AddMediatR(x =>
        {
            x.RegisterServicesFromAssemblies(typeof(Create.Command).Assembly,
                typeof(Create.CommandHandler).Assembly)
                .RegisterServicesFromAssemblies(typeof(Update.Command).Assembly,
                    typeof(Update.CommandHandler).Assembly)
                .RegisterServicesFromAssemblies(typeof(Delete.Command).Assembly,
                    typeof(Delete.CommandHandler).Assembly)
                .RegisterServicesFromAssemblies(typeof(GetAllArticles.Query).Assembly,
                    typeof(GetAllArticles.QueryHandler).Assembly)
                .RegisterServicesFromAssemblies(typeof(GetById.Query).Assembly,
                    typeof(GetById.QueryHandler).Assembly);
            
            x.AddOpenBehavior(typeof(QueryCachingBehavior<,>))
                //TODO .AddOpenBehavior(typeof(IdentityIdempotentCommandPipelineBehavior<,>))
                .AddOpenBehavior(typeof(RequestLoggingPipelineBehavior<,>))
                .AddOpenBehavior(typeof(ValidationBehaviour<,>))
                .AddOpenBehavior(typeof(MetricsBehaviour<,>));
            
            x.NotificationPublisher = new TaskWhenAllPublisher();
            x.NotificationPublisherType = typeof(TaskWhenAllPublisher);
        });

        return services;
    }
}