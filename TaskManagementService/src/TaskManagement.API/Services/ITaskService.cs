using TaskManagement.API.DTOs;

namespace TaskManagement.API.Services;

/// <summary>
/// Предоставляет сценарии управления задачами для слоя API.
/// </summary>
public interface ITaskService
{
    /// <summary>
    /// Возвращает все задачи.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Список задач.</returns>
    Task<IReadOnlyCollection<TaskResponse>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Возвращает задачу по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор задачи.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Ответ с задачей или null.</returns>
    Task<TaskResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Создает задачу.
    /// </summary>
    /// <param name="request">Данные создания.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Созданная задача.</returns>
    Task<TaskResponse> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Обновляет задачу.
    /// </summary>
    /// <param name="id">Идентификатор задачи.</param>
    /// <param name="request">Данные обновления.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Обновленная задача или null.</returns>
    Task<TaskResponse?> UpdateAsync(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Удаляет задачу.
    /// </summary>
    /// <param name="id">Идентификатор задачи.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Истина, если удаление выполнено.</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
