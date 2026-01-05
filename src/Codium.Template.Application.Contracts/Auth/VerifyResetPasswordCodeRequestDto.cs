using Codium.Template.Domain.Shared.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Codium.Template.Application.Contracts.Auth;

public class VerifyResetPasswordCodeRequestDto
{
    public string Email { get; set; } = null!;
    public string Code { get; set; } = null!;
}

public class VerifyResetPasswordCodeRequestDtoValidator : AbstractValidator<VerifyResetPasswordCodeRequestDto>
{
    public VerifyResetPasswordCodeRequestDtoValidator(IStringLocalizer<ApplicationResource> localizer)
    {
        RuleFor(item => item.Email)
            .NotEmpty().WithMessage(localizer["VerifyResetPasswordCodeRequestDto:Email:IsRequired"])
            .EmailAddress().WithMessage(localizer["VerifyResetPasswordCodeRequestDto:Email:Invalid"]);

        RuleFor(item => item.Code)
            .NotEmpty().WithMessage(localizer["VerifyResetPasswordCodeRequestDto:Code:IsRequired"])
            .MaximumLength(6).WithMessage(localizer["VerifyResetPasswordCodeRequestDto:Code:MaxLength", 6]);
    }
}

