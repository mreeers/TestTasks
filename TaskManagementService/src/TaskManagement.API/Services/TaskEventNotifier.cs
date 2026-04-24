using System.Net.Http.Json;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Integration;

namespace TaskManagement.API.Services;

/// <summary>
/// Отправляет события задач в сервис-наблюдатель по HTTP и RabbitMQ.
/// </summary>
public class TaskEventNotifier : ITaskEventNotifier
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITaskEventPublisher _taskEventPublisher;
    private readonly ILogger<TaskEventNotifier> _logger;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="TaskEventNotifier"/>.
    /// </summary>
    /// <param name="httpClientFactory">Фабрика HTTP-клиентов.</param>
    /// <param name="taskEventPublisher">Публикатор событий задач.</param>
    /// <param name="logger">Логгер.</param>
    public TaskEventNotifier(
        IHttpClientFactory httpClientFactory,
        ITaskEventPublisher taskEventPublisher,
        ILogger<TaskEventNotifier> logger)
    {
        _httpClientFactory = httpClientFactory;
        _taskEventPublisher = taskEventPublisher;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task NotifyAsync(TaskItem taskItem, string eventType, CancellationToken cancellationToken)
    {
        var taskChangedEvent = new TaskChangedEvent
        {
            Id = taskItem.Id,
            EventType = eventType,
            OccurredAt = DateTime.UtcNow
        };

        try
        {
            await _taskEventPublisher.PublishAsync(taskChangedEvent, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Failed to publish async RabbitMQ event for task {TaskId}",
                taskItem.Id);
        }

        try
        {
            var client = _httpClientFactory.CreateClient("ObserverClient");
            var response = await client.PostAsJsonAsync("/api/observer/task-changed", taskChangedEvent, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Failed to notify observer service synchronously for task {TaskId}",
                taskItem.Id);
        }
    }
}
