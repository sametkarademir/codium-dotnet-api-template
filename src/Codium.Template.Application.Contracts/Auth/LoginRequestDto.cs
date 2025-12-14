using Codium.Template.Domain.Shared.Localization;
using Codium.Template.Domain.Shared.Users;
using FluentValidation;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace Codium.Template.Application.Contracts.Auth;

public class LoginRequestDto
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestDtoValidator(IStringLocalizer<ApplicationResource> localizer, IOptions<IdentityUserOptions> options)
    {
        RuleFor(item => item.Email)
            .NotEmpty().WithMessage(localizer["LoginRequestDto:Email:IsRequired"])
            .EmailAddress().WithMessage(localizer["LoginRequestDto:Email:Invalid"])
            .MaximumLength(256).WithMessage(localizer["LoginRequestDto:Email:MaxLength", 256]);
        
        RuleFor(item => item.Password)
            .NotEmpty().WithMessage(localizer["LoginRequestDto:Password:IsRequired"])
            .MinimumLength(options.Value.Password.RequiredLength)
            .WithMessage(localizer["LoginRequestDto:Password:MinLength", options.Value.Password.RequiredLength])
            .MaximumLength(options.Value.Password.MaxLength)
            .WithMessage(localizer["LoginRequestDto:Password:MaxLength", options.Value.Password.MaxLength]);
    }
}