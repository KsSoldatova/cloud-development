using Event.Sink.Messaging;
using Event.Sink.Storage;

namespace Event.Sink;

/// <summary>
/// Экстеншен для добавления брокера в зависимости от конфигурации приложения
/// </summary>
internal static class WebApplicationExtensions
{
    /// <summary>
    /// Конфигурирует клиенские службы для взаимодействия с брокером сообщений
    /// </summary>
    /// <param name="app">Билдер</param>
    /// <returns>Билдер</returns>
    /// <exception cref="KeyNotFoundException">Если настройки не найдены</exception>
    public static async Task<WebApplication> UseConsumer(this WebApplication app)
    {
        await Task.CompletedTask;
        return app.Configuration.GetSection("Settings")["MessageBroker"] switch
        {
            "SQS" => app,
            _ => throw new KeyNotFoundException("Invalid broker type")
        };
    }

    /// <summary>
    /// Конфигурирует клиентские службы для взаимодействия с S3
    /// </summary>
    /// <param name="app">Билдер</param>
    /// <returns>Билдер</returns>
    public static async Task<WebApplication> UseS3(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var s3Service = scope.ServiceProvider.GetRequiredService<IS3Service>();
        await s3Service.EnsureBucketExists();
        return app;
    }
}