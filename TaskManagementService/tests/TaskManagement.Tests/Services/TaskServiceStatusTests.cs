using FluentAssertions;
using Moq;
using TaskManagement.API.DTOs;
using TaskManagement.API.Services;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;

namespace TaskManagement.Tests.Services;

/// <summary>
/// Contains unit tests for task status transitions in <see cref="TaskService"/>.
/// </summary>
public class TaskServiceStatusTests
{
    /// <summary>
    /// Verifies create trims fields, stores entity and sends created notification.
    /// </summary>
    [Fact]
    public async Task CreateAsync_ShouldCreateTask_AndNotifyCreatedEvent()
    {
        var repositoryMock = new Mock<ITaskRepository>();
        var notifierMock = new Mock<ITaskEventNotifier>();

        repositoryMock
            .Setup(repository => repository.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem taskItem, CancellationToken _) => taskItem);

        var service = new TaskService(repositoryMock.Object, notifierMock.Object);
        var request = new CreateTaskRequest
        {
            Title = "  New task  ",
            Description = "  New description  ",
            Status = TaskManagement.Domain.Enums.TaskStatus.New
        };

        var result = await service.CreateAsync(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.Title.Should().Be("New task");
        result.Description.Should().Be("New description");
        result.Status.Should().Be(TaskManagement.Domain.Enums.TaskStatus.New);

        repositoryMock.Verify(
            repository => repository.AddAsync(
                It.Is<TaskItem>(task =>
                    task.Title == "New task" &&
                    task.Description == "New description" &&
                    task.Status == TaskManagement.Domain.Enums.TaskStatus.New &&
                    task.Id != Guid.Empty),
                It.IsAny<CancellationToken>()),
            Times.Once);

        notifierMock.Verify(
            notifier => notifier.NotifyAsync(
                It.Is<TaskItem>(task => task.Title == "New task"),
                "Created",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies status change updates task and sends updated notification.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ShouldChangeStatus_AndNotifyUpdatedEvent()
    {
        var repositoryMock = new Mock<ITaskRepository>();
        var notifierMock = new Mock<ITaskEventNotifier>();

        var existingTask = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Old title",
            Description = "Old description",
            Status = TaskManagement.Domain.Enums.TaskStatus.New
        };

        repositoryMock
            .Setup(repository => repository.GetByIdAsync(existingTask.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTask);

        var service = new TaskService(repositoryMock.Object, notifierMock.Object);
        var request = new UpdateTaskRequest
        {
            Title = "Updated title",
            Description = "Updated description",
            Status = TaskManagement.Domain.Enums.TaskStatus.Completed
        };

        var result = await service.UpdateAsync(existingTask.Id, request, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Status.Should().Be(TaskManagement.Domain.Enums.TaskStatus.Completed);
        result.Title.Should().Be("Updated title");
        result.Description.Should().Be("Updated description");

        repositoryMock.Verify(
            repository => repository.UpdateAsync(
                It.Is<TaskItem>(task => task.Status == TaskManagement.Domain.Enums.TaskStatus.Completed),
                It.IsAny<CancellationToken>()),
            Times.Once);

        notifierMock.Verify(
            notifier => notifier.NotifyAsync(
                It.Is<TaskItem>(task => task.Id == existingTask.Id),
                "Updated",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies update returns null and does not notify when task is not found.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ShouldReturnNull_WhenTaskNotFound()
    {
        var repositoryMock = new Mock<ITaskRepository>();
        var notifierMock = new Mock<ITaskEventNotifier>();

        repositoryMock
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        var service = new TaskService(repositoryMock.Object, notifierMock.Object);
        var request = new UpdateTaskRequest
        {
            Title = "Updated title",
            Description = "Updated description",
            Status = TaskManagement.Domain.Enums.TaskStatus.InProgress
        };

        var result = await service.UpdateAsync(Guid.NewGuid(), request, CancellationToken.None);

        result.Should().BeNull();
        repositoryMock.Verify(
            repository => repository.UpdateAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()),
            Times.Never);
        notifierMock.Verify(
            notifier => notifier.NotifyAsync(It.IsAny<TaskItem>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// Verifies delete removes task and sends deleted notification.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_ShouldDeleteTask_AndNotifyDeletedEvent()
    {
        var repositoryMock = new Mock<ITaskRepository>();
        var notifierMock = new Mock<ITaskEventNotifier>();

        var existingTask = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Task",
            Description = "Description",
            Status = TaskManagement.Domain.Enums.TaskStatus.InProgress
        };

        repositoryMock
            .Setup(repository => repository.GetByIdAsync(existingTask.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTask);

        var service = new TaskService(repositoryMock.Object, notifierMock.Object);

        var result = await service.DeleteAsync(existingTask.Id, CancellationToken.None);

        result.Should().BeTrue();
        repositoryMock.Verify(
            repository => repository.DeleteAsync(existingTask, It.IsAny<CancellationToken>()),
            Times.Once);
        notifierMock.Verify(
            notifier => notifier.NotifyAsync(existingTask, "Deleted", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies delete returns false when task is not found.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenTaskNotFound()
    {
        var repositoryMock = new Mock<ITaskRepository>();
        var notifierMock = new Mock<ITaskEventNotifier>();

        repositoryMock
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        var service = new TaskService(repositoryMock.Object, notifierMock.Object);

        var result = await service.DeleteAsync(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeFalse();
        repositoryMock.Verify(
            repository => repository.DeleteAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()),
            Times.Never);
        notifierMock.Verify(
            notifier => notifier.NotifyAsync(It.IsAny<TaskItem>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
