using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Infrastructure.Persistence;
using TaskManagement.Infrastructure.Repositories;

namespace TaskManagement.Infrastructure.Extensions;

/// <summary>
/// Методы расширения for infrastructure service registration.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует infrastructure dependencies.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <param name="configuration">Конфигурация приложения.</param>
    /// <returns>Обновленная коллекция сервисов.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("TaskManagementDb");

        services.AddDbContext<TaskManagementDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddScoped<ITaskRepository, TaskRepository>();

        return services;
    }
}
