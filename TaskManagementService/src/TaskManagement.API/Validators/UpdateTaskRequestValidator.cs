using FluentValidation;
using TaskManagement.API.DTOs;

namespace TaskManagement.API.Validators;

/// <summary>
/// Валидирует тело запроса на обновление задачи.
/// </summary>
public class UpdateTaskRequestValidator : AbstractValidator<UpdateTaskRequest>
{
    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="UpdateTaskRequestValidator"/>.
    /// </summary>
    public UpdateTaskRequestValidator()
    {
        RuleFor(request => request.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(request => request.Description)
            .NotEmpty()
            .MaximumLength(2000);
    }
}
