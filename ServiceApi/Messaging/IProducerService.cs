using ServiceApi.Entities;

namespace ServiceApi.Messaging;

/// <summary>
/// Интерфейс службы для отправки сгенерированных программных проектов в брокер сообщений
/// </summary>
public interface IProducerService
{
    /// <summary>
    /// Отправляет сообщение в брокер
    /// </summary>
    /// <param name="programProject">Программный проект</param>
    public Task SendMessage(ProgramProject programProject);
}
