using Amazon.SQS;
using ServiceApi.Entities;
using System.Net;
using System.Text.Json;

namespace ServiceApi.Messaging;

/// <summary>
/// Служба для отправки сообщений в SQS
/// </summary>
/// <param name="client">Клиент SQS</param>
/// <param name="configuration">Конфигурация</param>
/// <param name="logger">Логгер</param>
public class SqsProducerService(IAmazonSQS client, IConfiguration configuration, ILogger<SqsProducerService> logger) : IProducerService
{
    private readonly string _queueName = configuration["AWS:Resources:SQSQueueName"]
        ?? throw new KeyNotFoundException("SQS queue link was not found in configuration");

    /// <inheritdoc/>
    public async Task SendMessage(ProgramProject programProject)
    {
        try
        {
            var json = JsonSerializer.Serialize(programProject);
            var response = await client.SendMessageAsync(_queueName, json);
            if (response.HttpStatusCode == HttpStatusCode.OK)
                logger.LogInformation("Программный проект {id} отправлен в очередь SQS", programProject.Id);
            else
                throw new Exception($"SQS вернул {response.HttpStatusCode}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Не удалось отправить программный проект через SQS");
        }
    }
}
