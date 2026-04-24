namespace TaskManagement.Domain.Entities;

/// <summary>
/// Представляет a task in the system.
/// </summary>
public class TaskItem
{
    /// <summary>
    /// Возвращает или задает the unique identifier of the task.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Возвращает или задает the title of the task.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Возвращает или задает the detailed description of the task.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Возвращает или задает the current status of the task.
    /// </summary>
    public Enums.TaskStatus Status { get; set; } = Enums.TaskStatus.New;

    /// <summary>
    /// Возвращает или задает task creation timestamp in UTC.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Возвращает или задает task last update timestamp in UTC.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
