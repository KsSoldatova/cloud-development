using ServiceApi.Entities;

namespace ServiceApi.Generator;

/// <summary>
/// Интерфейс для работы с кэшем проектов
/// </summary>
public interface IProgramProjectCache
{
    /// <summary>
    /// Получить проект из кэша по id
    /// </summary>
    public Task<ProgramProject?> GetProjectFromCache(int id);

    /// <summary>
    /// Сохранить проект в кэш
    /// </summary>
    public Task SaveProjectToCache(ProgramProject programProject);
}
