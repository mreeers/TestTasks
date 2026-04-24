using System.ComponentModel;
using Swashbuckle.AspNetCore.Annotations;

namespace TaskManagement.API.DTOs;

/// <summary>
/// Модель запроса на обновление задачи.
/// </summary>
public class UpdateTaskRequest
{
    /// <summary>
    /// Возвращает или задает заголовок задачи.
    /// </summary>
    [SwaggerSchema("Краткое название задачи. Обязательное поле, до 200 символов.")]
    [DefaultValue("Обновить отчет")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Возвращает или задает описание задачи.
    /// </summary>
    [SwaggerSchema("Подробное описание задачи. Обязательное поле, до 2000 символов.")]
    [DefaultValue("Добавить финальные правки и закрыть задачу.")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Возвращает или задает статус задачи.
    /// </summary>
    [SwaggerSchema("Статус задачи: 0 = New (новая), 1 = InProgress (в работе), 2 = Completed (выполненная).")]
    [DefaultValue(TaskManagement.Domain.Enums.TaskStatus.InProgress)]
    public TaskManagement.Domain.Enums.TaskStatus Status { get; set; }
}
