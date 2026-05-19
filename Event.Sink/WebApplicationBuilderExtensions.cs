using Amazon.S3;
using Amazon.SQS;
using Event.Sink.Messaging;
using Event.Sink.Storage;
using LocalStack.Client.Enums;
using LocalStack.Client.Extensions;

namespace Event.Sink;

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
    public static WebApplicationBuilder AddConsumer(this WebApplicationBuilder builder)
    {
        builder.Services.AddLocalStack(builder.Configuration);
        return builder.Configuration.GetSection("Settings")["MessageBroker"] switch
        {
            "SQS" => builder.AddSqsConsumer(),
            _ => throw new KeyNotFoundException("Invalid broker type. Expected SQS.")
        };
    }

    /// <summary>
    /// Регистрирует службы для работы с SQS
    /// </summary>
    /// <param name="builder">Билдер</param>
    /// <returns>Билдер</returns>
    private static WebApplicationBuilder AddSqsConsumer(this WebApplicationBuilder builder)
    {
        builder.Services.AddHostedService<SqsConsumerService>();
        builder.Services.AddAwsService<IAmazonSQS>();
        return builder;
    }

    /// <summary>
    /// Регистрирует клиентские службы для работы с объектным хранилищем
    /// </summary>
    /// <param name="builder">Билдер</param>
    /// <returns>Билдер</returns>
    /// <exception cref="KeyNotFoundException">Если настройки не найдены</exception>
    public static WebApplicationBuilder AddS3(this WebApplicationBuilder builder)
    {
        return builder.Configuration.GetSection("Settings")["S3Hosting"] switch
        {
            "Localstack" => builder.AddLocalstackS3(),
            _ => throw new KeyNotFoundException("Invalid s3 hosting type. Expected Localstack.")
        };
    }

    /// <summary>
    /// Регистрирует службы для работы с S3 через Localstack (AWS API)
    /// </summary>
    /// <param name="builder">Билдер</param>
    /// <returns>Билдер</returns>
    private static WebApplicationBuilder AddLocalstackS3(this WebApplicationBuilder builder)
    {
        builder.Services.AddAwsService<IAmazonS3>();
        builder.Services.AddScoped<IS3Service, S3AwsService>();
        return builder;
    }
}