using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Infrastructure.Persistence;

namespace TaskManagement.Infrastructure.Repositories;

/// <summary>
/// Реализация репозитория задач на EF Core.
/// </summary>
public class TaskRepository : ITaskRepository
{
    private readonly TaskManagementDbContext _dbContext;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="TaskRepository"/>.
    /// </summary>
    /// <param name="dbContext">Контекст базы данных.</param>
    public TaskRepository(TaskManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<TaskItem>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.TaskItems
            .AsNoTracking()
            .OrderByDescending(task => task.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.TaskItems.FirstOrDefaultAsync(task => task.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TaskItem> AddAsync(TaskItem taskItem, CancellationToken cancellationToken)
    {
        await _dbContext.TaskItems.AddAsync(taskItem, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return taskItem;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(TaskItem taskItem, CancellationToken cancellationToken)
    {
        _dbContext.TaskItems.Update(taskItem);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(TaskItem taskItem, CancellationToken cancellationToken)
    {
        _dbContext.TaskItems.Remove(taskItem);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
