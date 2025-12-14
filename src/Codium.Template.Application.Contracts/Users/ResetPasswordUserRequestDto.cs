using Codium.Template.Domain.Shared.Localization;
using Codium.Template.Domain.Shared.Users;
using FluentValidation;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace Codium.Template.Application.Contracts.Users;

public class ResetPasswordUserRequestDto
{
    public string NewPassword { get; set; } = null!;
    public string ConfirmNewPassword { get; set; } = null!;
}

public class ResetPasswordUserRequestDtoValidator : AbstractValidator<ResetPasswordUserRequestDto>
{
    public ResetPasswordUserRequestDtoValidator(IStringLocalizer<ApplicationResource> localizer, IOptions<IdentityUserOptions> options)
    {
        RuleFor(item => item.NewPassword)
            .NotEmpty().WithMessage(localizer["ResetPasswordUserRequestDto:Password:IsRequired"])
            .MinimumLength(options.Value.Password.RequiredLength).WithMessage(localizer["ResetPasswordUserRequestDto:Password:MinLength", options.Value.Password.RequiredLength])
            .MaximumLength(options.Value.Password.MaxLength).WithMessage(localizer["ResetPasswordUserRequestDto:Password:MaxLength", options.Value.Password.MaxLength]);
        
        RuleFor(item => item.ConfirmNewPassword)
            .Equal(item => item.NewPassword).WithMessage(localizer["ResetPasswordUserRequestDto:ConfirmPassword:MustMatchPassword"]);
    }
}