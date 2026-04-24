using TaskManagement.API.DTOs;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;

namespace TaskManagement.API.Services;

/// <summary>
/// Реализует бизнес-операции с задачами.
/// </summary>
public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly ITaskEventNotifier _taskEventNotifier;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="TaskService"/>.
    /// </summary>
    /// <param name="taskRepository">Репозиторий задач.</param>
    /// <param name="taskEventNotifier">Сервис уведомлений о событиях задач.</param>
    public TaskService(ITaskRepository taskRepository, ITaskEventNotifier taskEventNotifier)
    {
        _taskRepository = taskRepository;
        _taskEventNotifier = taskEventNotifier;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<TaskResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var tasks = await _taskRepository.GetAllAsync(cancellationToken);
        return tasks.Select(MapToResponse).ToList();
    }

    /// <inheritdoc />
    public async Task<TaskResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var taskItem = await _taskRepository.GetByIdAsync(id, cancellationToken);
        return taskItem is null ? null : MapToResponse(taskItem);
    }

    /// <inheritdoc />
    public async Task<TaskResponse> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var taskItem = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Status = request.Status
        };

        var createdTask = await _taskRepository.AddAsync(taskItem, cancellationToken);
        await _taskEventNotifier.NotifyAsync(createdTask, "Created", cancellationToken);
        return MapToResponse(createdTask);
    }

    /// <inheritdoc />
    public async Task<TaskResponse?> UpdateAsync(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        var taskItem = await _taskRepository.GetByIdAsync(id, cancellationToken);
        if (taskItem is null)
        {
            return null;
        }

        taskItem.Title = request.Title.Trim();
        taskItem.Description = request.Description.Trim();
        taskItem.Status = request.Status;

        await _taskRepository.UpdateAsync(taskItem, cancellationToken);
        await _taskEventNotifier.NotifyAsync(taskItem, "Updated", cancellationToken);

        return MapToResponse(taskItem);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var taskItem = await _taskRepository.GetByIdAsync(id, cancellationToken);
        if (taskItem is null)
        {
            return false;
        }

        await _taskRepository.DeleteAsync(taskItem, cancellationToken);
        await _taskEventNotifier.NotifyAsync(taskItem, "Deleted", cancellationToken);

        return true;
    }

    private static TaskResponse MapToResponse(TaskItem taskItem)
    {
        return new TaskResponse
        {
            Id = taskItem.Id,
            Title = taskItem.Title,
            Description = taskItem.Description,
            Status = taskItem.Status,
            CreatedAt = taskItem.CreatedAt,
            UpdatedAt = taskItem.UpdatedAt
        };
    }
}
