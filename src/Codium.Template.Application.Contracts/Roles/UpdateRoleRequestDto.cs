using Codium.Template.Domain.Shared.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Codium.Template.Application.Contracts.Roles;

public class UpdateRoleRequestDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}

public class UpdateRoleRequestDtoValidator : AbstractValidator<UpdateRoleRequestDto>
{
    public UpdateRoleRequestDtoValidator(IStringLocalizer<ApplicationResource> localizer)
    {
        RuleFor(item => item.Name)
            .NotEmpty().WithMessage(localizer["UpdateRoleRequestDto:Name:NotEmpty"])
            .MaximumLength(256).WithMessage(localizer["UpdateRoleRequestDto:Name:MaxLength", 256]);
        
        RuleFor(item => item.Description)
            .MaximumLength(2048).WithMessage(localizer["UpdateRoleRequestDto:Description:MaxLength", 2048]);
    }
}