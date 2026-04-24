using FluentAssertions;
using TaskManagement.API.DTOs;
using TaskManagement.API.Validators;

namespace TaskManagement.Tests.Validators;

/// <summary>
/// Contains unit tests for task request validators.
/// </summary>
public class TaskRequestValidatorsTests
{
    /// <summary>
    /// Verifies create validator fails when title is empty.
    /// </summary>
    [Fact]
    public void CreateValidator_ShouldFail_WhenTitleIsEmpty()
    {
        var validator = new CreateTaskRequestValidator();
        var request = new CreateTaskRequest
        {
            Title = string.Empty,
            Description = "Valid description",
            Status = TaskManagement.Domain.Enums.TaskStatus.New
        };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(CreateTaskRequest.Title));
    }

    /// <summary>
    /// Verifies create validator fails when description exceeds max length.
    /// </summary>
    [Fact]
    public void CreateValidator_ShouldFail_WhenDescriptionTooLong()
    {
        var validator = new CreateTaskRequestValidator();
        var request = new CreateTaskRequest
        {
            Title = "Valid title",
            Description = new string('a', 2001),
            Status = TaskManagement.Domain.Enums.TaskStatus.New
        };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(CreateTaskRequest.Description));
    }

    /// <summary>
    /// Verifies update validator succeeds for valid payload.
    /// </summary>
    [Fact]
    public void UpdateValidator_ShouldPass_ForValidRequest()
    {
        var validator = new UpdateTaskRequestValidator();
        var request = new UpdateTaskRequest
        {
            Title = "Updated title",
            Description = "Updated description",
            Status = TaskManagement.Domain.Enums.TaskStatus.Completed
        };

        var result = validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Verifies update validator fails when description is empty.
    /// </summary>
    [Fact]
    public void UpdateValidator_ShouldFail_WhenDescriptionIsEmpty()
    {
        var validator = new UpdateTaskRequestValidator();
        var request = new UpdateTaskRequest
        {
            Title = "Updated title",
            Description = string.Empty,
            Status = TaskManagement.Domain.Enums.TaskStatus.InProgress
        };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(UpdateTaskRequest.Description));
    }
}
