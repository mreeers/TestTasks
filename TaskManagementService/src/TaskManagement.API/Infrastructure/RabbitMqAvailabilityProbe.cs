using System.Net.Sockets;

namespace TaskManagement.API.Infrastructure;

/// <summary>
/// Предоставляет низкоуровневую проверку TCP-доступности хоста RabbitMQ.
/// </summary>
public static class RabbitMqAvailabilityProbe
{
    /// <summary>
    /// Проверяет доступность хоста RabbitMQ на целевом TCP-порту.
    /// </summary>
    /// <param name="host">Хост RabbitMQ.</param>
    /// <param name="port">Порт RabbitMQ.</param>
    /// <param name="timeoutMs">Таймаут подключения в миллисекундах.</param>
    /// <returns>Истина, если TCP-подключение может быть установлено.</returns>
    public static async Task<bool> IsReachableAsync(string host, int port, int timeoutMs = 1500)
    {
        try
        {
            using var tcpClient = new TcpClient();
            using var cancellationSource = new CancellationTokenSource(timeoutMs);
            await tcpClient.ConnectAsync(host, port, cancellationSource.Token);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
