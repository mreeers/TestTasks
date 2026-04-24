using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Persistence;

/// <summary>
/// EF Core database context for task management module.
/// </summary>
public class TaskManagementDbContext : DbContext
{
    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="TaskManagementDbContext"/>.
    /// </summary>
    /// <param name="options">Параметры контекста.</param>
    public TaskManagementDbContext(DbContextOptions<TaskManagementDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Возвращает таблицу задач.
    /// </summary>
    public DbSet<TaskItem> TaskItems => Set<TaskItem>();

    /// <summary>
    /// Сохраняет изменения и обновляет технические временные метки.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>A task representing async operation.</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Сохраняет изменения и обновляет технические временные метки.
    /// </summary>
    /// <returns>The number of affected records.</returns>
    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    /// <summary>
    /// Применяет конфигурации сущностей.
    /// </summary>
    /// <param name="modelBuilder">Построитель модели.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TaskManagementDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    private void UpdateAuditFields()
    {
        var utcNow = DateTime.UtcNow;

        var entries = ChangeTracker
            .Entries<TaskItem>()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified);

        foreach (var entry in entries)
        {
            entry.Entity.UpdatedAt = utcNow;
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = utcNow;
            }
        }
    }
}
