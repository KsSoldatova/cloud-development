using System.Text.Json.Nodes;

namespace FileService.Storage;

/// <summary>
/// Интерфейс службы для работы с файлами в объектном хранилище
/// </summary>
public interface IS3Service
{
    /// <summary>
    /// Отправляет файл в хранилище
    /// </summary>
    /// <param name="fileData">Строковая репрезентация сохраняемого файла</param>
    public Task<bool> UploadFile(string fileData);

    /// <summary>
    /// Получает список всех файлов из хранилища
    /// </summary>
    public Task<List<string>> GetFileList();

    /// <summary>
    /// Получает строковую репрезентацию файла из хранилища
    /// </summary>
    /// <param name="key">Ключ файла в бакете</param>
    public Task<JsonNode> DownloadFile(string key);

    /// <summary>
    /// Создает бакет, если его еще нет
    /// </summary>
    public Task EnsureBucketExists();
}
