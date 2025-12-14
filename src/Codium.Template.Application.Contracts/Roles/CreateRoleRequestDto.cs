using Codium.Template.Domain.Shared.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Codium.Template.Application.Contracts.Roles;

public class CreateRoleRequestDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}

public class CreateRoleRequestDtoValidator : AbstractValidator<CreateRoleRequestDto>
{
    public CreateRoleRequestDtoValidator(IStringLocalizer<ApplicationResource> localizer)
    {
        RuleFor(item => item.Name)
            .NotEmpty().WithMessage(localizer["Role:Name:NotEmpty"])
            .MaximumLength(256).WithMessage(localizer["Role:Name:MaxLength", 256]);
        
        RuleFor(item => item.Description)
            .MaximumLength(2048).WithMessage(localizer["Role:Description:MaxLength", 2048]);
    }
}