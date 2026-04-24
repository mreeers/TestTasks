using Microsoft.AspNetCore.Mvc;
using TaskManagement.Domain.Integration;

namespace TaskManagement.Observer.Controllers;

/// <summary>
/// Принимает synchronous notifications about task changes.
/// </summary>
[ApiController]
[Route("api/observer")]
public class ObserverController : ControllerBase
{
    private readonly ILogger<ObserverController> _logger;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="ObserverController"/>.
    /// </summary>
    /// <param name="logger">Логгер.</param>
    public ObserverController(ILogger<ObserverController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Принимает task changed event from Task service over HTTP.
    /// </summary>
    /// <param name="request">Полезная нагрузка события изменения задачи.</param>
    /// <returns>Ответ HTTP 200.</returns>
    [HttpPost("task-changed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult NotifyTaskChanged([FromBody] TaskChangedEvent request)
    {
        _logger.LogInformation(
            "[Sync Event Log]: Task {TaskId} updated at {Time}. EventType={EventType}",
            request.Id,
            request.OccurredAt,
            request.EventType);

        return Ok();
    }
}
