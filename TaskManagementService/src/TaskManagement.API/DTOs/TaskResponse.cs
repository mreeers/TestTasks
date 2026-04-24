namespace TaskManagement.API.DTOs;

/// <summary>
/// Модель ответа с данными задачи.
/// </summary>
public class TaskResponse
{
    /// <summary>
    /// Возвращает или задает идентификатор задачи.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Возвращает или задает заголовок задачи.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Возвращает или задает описание задачи.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Возвращает или задает статус задачи.
    /// </summary>
    public TaskManagement.Domain.Enums.TaskStatus Status { get; set; }

    /// <summary>
    /// Возвращает или задает время создания задачи.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Возвращает или задает время обновления задачи.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
