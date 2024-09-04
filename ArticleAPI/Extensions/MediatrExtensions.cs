using Application.Core.Behaviours;
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
            x.RegisterServicesFromAssemblyContaining<Program>();

            //x.RegisterServicesFromAssemblies(typeof(Register.Command).Assembly,
            //    typeof(Register.CommandHandler).Assembly);
            
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