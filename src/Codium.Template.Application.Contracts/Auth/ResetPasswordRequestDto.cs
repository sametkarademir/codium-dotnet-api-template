using Codium.Template.Domain.Shared.Localization;
using Codium.Template.Domain.Shared.Users;
using FluentValidation;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

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
    public ResetPasswordRequestDtoValidator(IStringLocalizer<ApplicationResource> localizer, IOptions<IdentityUserOptions> options)
    {
        RuleFor(item => item.Email)
            .NotEmpty().WithMessage(localizer["ResetPasswordRequestDto:Email:IsRequired"])
            .EmailAddress().WithMessage(localizer["ResetPasswordRequestDto:Email:Invalid"]);

        RuleFor(item => item.Code)
            .NotEmpty().WithMessage(localizer["ResetPasswordRequestDto:Code:IsRequired"])
            .MaximumLength(1024).WithMessage(localizer["ResetPasswordRequestDto:Code:MaxLength", 1024]);

        RuleFor(item => item.NewPassword)
            .NotEmpty().WithMessage(localizer["ResetPasswordRequestDto:NewPassword:IsRequired"])
            .MinimumLength(options.Value.Password.RequiredLength)
            .WithMessage(localizer["ResetPasswordRequestDto:NewPassword:MinLength", options.Value.Password.RequiredLength])
            .MaximumLength(options.Value.Password.MaxLength)
            .WithMessage(localizer["ResetPasswordRequestDto:NewPassword:MaxLength", options.Value.Password.MaxLength]);
        
        RuleFor(item => item.ConfirmNewPassword)
            .Equal(item => item.NewPassword).WithMessage(localizer["ResetPasswordRequestDto:ConfirmNewPassword:DoesNotMatch"]);
    }
}