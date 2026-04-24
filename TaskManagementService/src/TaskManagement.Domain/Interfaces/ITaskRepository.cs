using TaskManagement.Domain.Entities;

namespace TaskManagement.Domain.Interfaces;

/// <summary>
/// Предоставляет CRUD-операции для задач.
/// </summary>
public interface ITaskRepository
{
    /// <summary>
    /// Возвращает все задачи.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Коллекция задач.</returns>
    Task<IReadOnlyCollection<TaskItem>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Возвращает задачу по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор задачи.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Найденная задача или null.</returns>
    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Добавляет новую задачу.
    /// </summary>
    /// <param name="taskItem">Сущность задачи.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Созданная сущность задачи.</returns>
    Task<TaskItem> AddAsync(TaskItem taskItem, CancellationToken cancellationToken);

    /// <summary>
    /// Обновляет существующую задачу.
    /// </summary>
    /// <param name="taskItem">Сущность задачи.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task UpdateAsync(TaskItem taskItem, CancellationToken cancellationToken);

    /// <summary>
    /// Удаляет задачу по идентификатору.
    /// </summary>
    /// <param name="taskItem">Сущность задачи.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task DeleteAsync(TaskItem taskItem, CancellationToken cancellationToken);
}
