using Codium.Template.Domain.Shared.Localization;
using Codium.Template.Domain.Shared.Users;
using FluentValidation;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace Codium.Template.Application.Contracts.Profiles;

public class ChangePasswordUserRequestDto
{
    public string OldPassword { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
    public string ConfirmNewPassword { get; set; } = null!;
}

public class ChangePasswordUserRequestDtoValidator : AbstractValidator<ChangePasswordUserRequestDto>
{
    public ChangePasswordUserRequestDtoValidator(IStringLocalizer<ApplicationResource> localizer, IOptions<IdentityUserOptions> options)
    {
        RuleFor(item => item.OldPassword)
            .NotEmpty().WithMessage(localizer["ChangePasswordUserRequestDto:Password:IsRequired"])
            .MinimumLength(options.Value.Password.RequiredLength).WithMessage(localizer["ChangePasswordUserRequestDto:Password:MinLength", options.Value.Password.RequiredLength])
            .MaximumLength(options.Value.Password.MaxLength).WithMessage(localizer["ChangePasswordUserRequestDto:Password:MaxLength", options.Value.Password.MaxLength]);

        RuleFor(item => item.NewPassword)
            .NotEmpty().WithMessage(localizer["ChangePasswordUserRequestDto:Password:IsRequired"])
            .MinimumLength(options.Value.Password.RequiredLength).WithMessage(localizer["ChangePasswordUserRequestDto:Password:MinLength", options.Value.Password.RequiredLength])
            .MaximumLength(options.Value.Password.MaxLength).WithMessage(localizer["ChangePasswordUserRequestDto:Password:MaxLength", options.Value.Password.MaxLength]);
        
        RuleFor(item => item.ConfirmNewPassword)
            .Equal(item => item.NewPassword).WithMessage(localizer["ChangePasswordUserRequestDto:PasswordConfirm:MustMatchPassword"]);
    }
}