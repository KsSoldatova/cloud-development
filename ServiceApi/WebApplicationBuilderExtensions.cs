using Amazon.SQS;
using LocalStack.Client.Extensions;
using ServiceApi.Messaging;

namespace ServiceApi;

/// <summary>
/// Экстеншен для добавления различных служб в DI в зависимости от конфигурации приложения
/// </summary>
internal static class WebApplicationBuilderExtensions
{
    /// <summary>
    /// Регистрирует клиентские службы для работы с брокером сообщений
    /// </summary>
    /// <param name="builder">Билдер</param>
    /// <returns>Билдер</returns>
    /// <exception cref="KeyNotFoundException">Если настройки не найдены</exception>
    public static WebApplicationBuilder AddProducer(this WebApplicationBuilder builder)
    {
        builder.Services.AddLocalStack(builder.Configuration);
        return builder.Configuration.GetSection("Settings")["MessageBroker"] switch
        {
            "SQS" => builder.AddSqsProducer(),
            _ => throw new KeyNotFoundException("Invalid broker type. Expected SQS.")
        };
    }

    /// <summary>
    /// Регистрирует службы для работы с SQS
    /// </summary>
    /// <param name="builder">Билдер</param>
    /// <returns>Билдер</returns>
    private static WebApplicationBuilder AddSqsProducer(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IProducerService, SqsProducerService>();
        builder.Services.AddAwsService<IAmazonSQS>();
        return builder;
    }
}