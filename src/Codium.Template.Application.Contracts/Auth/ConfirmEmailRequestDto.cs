using Codium.Template.Domain.Shared.ConfirmationCodes;
using Codium.Template.Domain.Shared.Localization;
using Codium.Template.Domain.Shared.Users;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Codium.Template.Application.Contracts.Auth;

public class ConfirmEmailRequestDto
{
    public string Email { get; set; } = null!;
    public string Code { get; set; } = null!;
}

public class ConfirmEmailRequestDtoValidator : AbstractValidator<ConfirmEmailRequestDto>
{
    public ConfirmEmailRequestDtoValidator(IStringLocalizer<ApplicationResource> localizer)
    {
        RuleFor(item => item.Email)
            .NotEmpty().WithMessage(localizer["ConfirmEmailRequestDto:Email:IsRequired"])
            .MaximumLength(UserConsts.EmailMaxLength).WithMessage(localizer["ConfirmEmailRequestDto:Email:MaxLength", UserConsts.EmailMaxLength])
            .EmailAddress().WithMessage(localizer["ConfirmEmailRequestDto:Email:Invalid"]);

        RuleFor(item => item.Code)
            .NotEmpty().WithMessage(localizer["ConfirmEmailRequestDto:Code:IsRequired"])
            .MaximumLength(ConfirmationCodeConsts.CodeMaxLength).WithMessage(localizer["ConfirmEmailRequestDto:Code:MaxLength", ConfirmationCodeConsts.CodeMaxLength]);
    }
}