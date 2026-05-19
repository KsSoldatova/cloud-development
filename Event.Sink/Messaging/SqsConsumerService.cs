using Amazon.SQS;
using Amazon.SQS.Model;
using Event.Sink.Storage;

namespace Event.Sink.Messaging;

/// <summary>
/// Клиентская служба для приема сообщений из очереди SQS
/// </summary>
public class SqsConsumerService(IAmazonSQS sqsClient,
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<SqsConsumerService> logger) : BackgroundService
{
    private readonly string _queueName = configuration["AWS:Resources:SQSQueueName"]
        ?? throw new KeyNotFoundException("SQS queue name was not found in configuration");

    private string? _queueUrl;
    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("SQS consumer service started.");
        var getUrlResponse = await sqsClient.GetQueueUrlAsync(_queueName, stoppingToken);
        _queueUrl = getUrlResponse.QueueUrl;
        logger.LogInformation("Queue URL: {QueueUrl}", _queueUrl);

        while (!stoppingToken.IsCancellationRequested)
        {
            var response = await sqsClient.ReceiveMessageAsync(
                new ReceiveMessageRequest
                {
                    QueueUrl = _queueUrl,
                    MaxNumberOfMessages = 10, 
                    WaitTimeSeconds = 5 
                }, stoppingToken);

            if (response?.Messages == null || response.Messages.Count == 0)
            {
                continue;
            }

            logger.LogInformation("Received {count} messages", response.Messages.Count);

            if (response.Messages != null)
            {

                foreach (var message in response.Messages)
                {
                    try
                    {
                        logger.LogInformation("Processing message: {messageId}", message.MessageId);

                        using var scope = scopeFactory.CreateScope();
                        var s3Service = scope.ServiceProvider.GetRequiredService<IS3Service>();
                        await s3Service.UploadFile(message.Body);

                        await sqsClient.DeleteMessageAsync(_queueUrl, message.ReceiptHandle, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error processing message: {messageId}", message.MessageId);
                        continue;
                    }
                }
                logger.LogInformation("Batch of {count} messages processed", response.Messages.Count);
            }
        }
    }
}