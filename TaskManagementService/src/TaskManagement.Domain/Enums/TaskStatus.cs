namespace TaskManagement.Domain.Enums;

/// <summary>
/// Представляет the lifecycle status of a task.
/// </summary>
public enum TaskStatus
{
    /// <summary>
    /// Task is created and not started yet.
    /// </summary>
    New = 0,

    /// <summary>
    /// Task is currently in progress.
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Task is completed.
    /// </summary>
    Completed = 2
}
