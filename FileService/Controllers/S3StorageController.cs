using FileService.Storage;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;

namespace FileService.Controllers;

/// <summary>
/// Контроллер для взаимодействия с S3
/// </summary>
/// <param name="s3Service">Служба для работы с S3</param>
/// <param name="logger">Логгер</param>
[ApiController]
[Route("api/s3")]
public class S3StorageController(IS3Service s3Service, ILogger<S3StorageController> logger) : ControllerBase
{
    /// <summary>
    /// Получение списка хранящихся в S3 файлов
    /// </summary>
    [HttpGet]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<List<string>>> ListFiles()
    {
        try
        {
            var list = await s3Service.GetFileList();
            logger.LogInformation("Получен список из {count} файлов", list.Count);
            return Ok(list);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при получении списка файлов");
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Получение содержимого хранящегося в S3 документа
    /// </summary>
    /// <param name="key">Ключ файла</param>
    [HttpGet("{key}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<JsonNode>> GetFile(string key)
    {
        try
        {
            var node = await s3Service.DownloadFile(key);
            return Ok(node);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при скачивании файла {key}", key);
            return BadRequest(ex.Message);
        }
    }
}
