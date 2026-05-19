using Amazon.S3;
using Amazon.S3.Model;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace FileService.Storage;

/// <summary>
/// Служба для работы с файлами в объектном хранилище S3 (LocalStack)
/// </summary>
/// <param name="client">S3 клиент</param>
/// <param name="configuration">Конфигурация</param>
/// <param name="logger">Логер</param>
public class S3AwsService(IAmazonS3 client, IConfiguration configuration, ILogger<S3AwsService> logger) : IS3Service
{
    private readonly string _bucketName = configuration["AWS:Resources:S3BucketName"]
        ?? throw new KeyNotFoundException("S3 bucket name was not found in configuration");

    /// <inheritdoc/>
    public async Task<List<string>> GetFileList()
    {
        var list = new List<string>();
        var request = new ListObjectsV2Request
        {
            BucketName = _bucketName,
            Prefix = "",
            Delimiter = ",",
        };
        var paginator = client.Paginators.ListObjectsV2(request);

        logger.LogInformation("Получение списка файлов из {bucket}", _bucketName);
        await foreach (var response in paginator.Responses)
            if (response?.S3Objects != null)
                foreach (var obj in response.S3Objects)
                    if (obj != null)
                        list.Add(obj.Key);
        return list;
    }

    /// <inheritdoc/>
    public async Task<bool> UploadFile(string fileData)
    {
        var rootNode = JsonNode.Parse(fileData) ?? throw new ArgumentException("Переданная строка не является валидным JSON");
        var id = rootNode["id"]?.GetValue<int>() ?? throw new ArgumentException("Переданный JSON имеет неверную структуру");

        using var stream = new MemoryStream();
        JsonSerializer.Serialize(stream, rootNode);
        stream.Seek(0, SeekOrigin.Begin);

        logger.LogInformation("Загрузка программного проекта {file} в {bucket}", id, _bucketName);
        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = $"programproject_{id}.json",
            InputStream = stream
        };

        var response = await client.PutObjectAsync(request);

        if (response.HttpStatusCode != HttpStatusCode.OK)
        {
            logger.LogError("Не удалось загрузить программный проект {file}: {code}", id, response.HttpStatusCode);
            return false;
        }
        logger.LogInformation("Программный проект {file} загружен в {bucket}", id, _bucketName);
        return true;
    }

    /// <inheritdoc/>
    public async Task<JsonNode> DownloadFile(string key)
    {
        logger.LogInformation("Скачивание {file} из {bucket}", key, _bucketName);
        var request = new GetObjectRequest
        {
            BucketName = _bucketName,
            Key = key
        };
        using var response = await client.GetObjectAsync(request);

        if (response.HttpStatusCode != HttpStatusCode.OK)
            throw new InvalidOperationException($"Ошибка при скачивании {key} - {response.HttpStatusCode}");

        using var reader = new StreamReader(response.ResponseStream, Encoding.UTF8);
        return JsonNode.Parse(await reader.ReadToEndAsync())
            ?? throw new InvalidOperationException("Скачанный документ не является валидным JSON");
    }

    /// <inheritdoc/>
    public async Task EnsureBucketExists()
    {
        logger.LogInformation("Проверка существования {bucket}", _bucketName);
        await client.EnsureBucketExistsAsync(_bucketName);
        logger.LogInformation("Существование {bucket} подтверждено", _bucketName);
    }
}
