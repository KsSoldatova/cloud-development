using System.Text.Json.Serialization;

namespace ServiceApi.Entities;

public class ProgramProject
{
    /// <summary>
    /// Идентификатор
    /// </summary>
    [JsonPropertyName("id")]
    public required int Id { get; set; }

    /// <summary>
    /// Название проекта
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    /// Заказчик проекта
    /// </summary>
    [JsonPropertyName("customer")]
    public required string Customer { get; set; }

    /// <summary>
    /// Менеджер проекта
    /// </summary>
    [JsonPropertyName("manager")]
    public required string Manager { get; set; }

    /// <summary>
    /// Дата начала
    /// </summary>
    [JsonPropertyName("startDate")]
    public required DateOnly StartDate { get; set; }

    /// <summary>
    /// Плановая дата завершения
    /// </summary>
    [JsonPropertyName("planEndDate")]
    public required DateOnly PlanEndDate { get; set; }

    /// <summary>
    /// Фактическая дата завершения
    /// </summary>
    [JsonPropertyName("actualEndDate")]
    public DateOnly? ActualEndDate { get; set; }

    /// <summary>
    /// Бюджет
    /// </summary>
    [JsonPropertyName("budget")]
    public required decimal Budget { get; set; }

    /// <summary>
    /// Фактические затраты
    /// </summary>
    [JsonPropertyName("actualCost")]
    public required decimal ActualCost { get; set; }

    /// <summary>
    /// Процент выполнения
    /// </summary>
    [JsonPropertyName("percentComplete")]
    public required int PercentComplete { get; set; }
}