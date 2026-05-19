using Amazon.SQS;
using Amazon.SQS.Model;
using FileService.Storage;

namespace FileService.Messaging;

/// <summary>
/// Клиентская служба для приема сообщений из очереди SQS и сохранения их в S3
/// </summary>
/// <param name="sqsClient">Клиент SQS</param>
/// <param name="scopeFactory">Фабрика контекста</param>
/// <param name="configuration">Конфигурация</param>
/// <param name="logger">Логгер</param>
public class SqsConsumerService(
    IAmazonSQS sqsClient,
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<SqsConsumerService> logger) : BackgroundService
{
    private readonly string _queueName = configuration["AWS:Resources:SQSQueueName"]
        ?? throw new KeyNotFoundException("SQS queue name was not found in configuration");

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Служба SQS-консьюмера запущена.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var response = await sqsClient.ReceiveMessageAsync(
                new ReceiveMessageRequest
                {
                    QueueUrl = _queueName,
                    MaxNumberOfMessages = 10,
                    WaitTimeSeconds = 5
                }, stoppingToken);

            if (response.Messages is null)
                continue;

            logger.LogInformation("Получено {count} сообщений", response.Messages.Count);

            foreach (var message in response.Messages)
            {
                try
                {
                    logger.LogInformation("Обработка сообщения: {messageId}", message.MessageId);

                    using var scope = scopeFactory.CreateScope();
                    var s3Service = scope.ServiceProvider.GetRequiredService<IS3Service>();
                    await s3Service.UploadFile(message.Body);

                    await sqsClient.DeleteMessageAsync(_queueName, message.ReceiptHandle, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Ошибка обработки сообщения: {messageId}", message.MessageId);
                }
            }
        }
    }
}
