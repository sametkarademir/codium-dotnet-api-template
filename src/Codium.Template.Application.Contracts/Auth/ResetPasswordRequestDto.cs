using Codium.Template.Domain.Shared.ConfirmationCodes;
using Codium.Template.Domain.Shared.Localization;
using Codium.Template.Domain.Shared.Users;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Codium.Template.Application.Contracts.Auth;

public class ResetPasswordRequestDto
{
    public string Code { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
    public string ConfirmNewPassword { get; set; } = null!;
}

public class ResetPasswordRequestDtoValidator : AbstractValidator<ResetPasswordRequestDto>
{
    public ResetPasswordRequestDtoValidator(IStringLocalizer<ApplicationResource> localizer)
    {
        RuleFor(item => item.Email)
            .NotEmpty().WithMessage(localizer["ResetPasswordRequestDto:Email:IsRequired"])
            .EmailAddress().WithMessage(localizer["ResetPasswordRequestDto:Email:Invalid"])
            .MaximumLength(UserConsts.EmailMaxLength).WithMessage(localizer["ResetPasswordRequestDto:Email:MaxLength", UserConsts.EmailMaxLength]);

        RuleFor(item => item.Code)
            .NotEmpty().WithMessage(localizer["ResetPasswordRequestDto:Code:IsRequired"])
            .MaximumLength(ConfirmationCodeConsts.CodeMaxLength).WithMessage(localizer["ResetPasswordRequestDto:Code:MaxLength", ConfirmationCodeConsts.CodeMaxLength]);

        RuleFor(item => item.NewPassword)
            .NotEmpty().WithMessage(localizer["ResetPasswordRequestDto:NewPassword:IsRequired"])
            .MinimumLength(UserConsts.PasswordRequiredLength)
            .WithMessage(localizer["ResetPasswordRequestDto:NewPassword:MinLength", UserConsts.PasswordRequiredLength])
            .MaximumLength(UserConsts.PasswordMaxLength)
            .WithMessage(localizer["ResetPasswordRequestDto:NewPassword:MaxLength", UserConsts.PasswordMaxLength]);
        
        RuleFor(item => item.ConfirmNewPassword)
            .Equal(item => item.NewPassword).WithMessage(localizer["ResetPasswordRequestDto:ConfirmNewPassword:DoesNotMatch"]);
    }
}