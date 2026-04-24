using FluentValidation;
using TaskManagement.API.DTOs;

namespace TaskManagement.API.Validators;

/// <summary>
/// Валидирует тело запроса на создание задачи.
/// </summary>
public class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="CreateTaskRequestValidator"/>.
    /// </summary>
    public CreateTaskRequestValidator()
    {
        RuleFor(request => request.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(request => request.Description)
            .NotEmpty()
            .MaximumLength(2000);
    }
}
