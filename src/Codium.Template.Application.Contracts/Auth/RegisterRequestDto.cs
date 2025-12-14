using Codium.Template.Domain.Shared.Localization;
using Codium.Template.Domain.Shared.Users;
using FluentValidation;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace Codium.Template.Application.Contracts.Auth;

public class RegisterRequestDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }

    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string ConfirmPassword { get; set; } = null!;
}

public class RegisterRequestDtoValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestDtoValidator(IStringLocalizer<ApplicationResource> localizer, IOptions<IdentityUserOptions> options)
    {
        RuleFor(item => item.FirstName)
            .MaximumLength(128).WithMessage(localizer["RegisterRequestDto:FirstName:MaxLength", 128]);

        RuleFor(item => item.LastName)
            .MaximumLength(128).WithMessage(localizer["RegisterRequestDto:LastName:MaxLength", 128]);

        RuleFor(item => item.Email)
            .NotEmpty().WithMessage(localizer["RegisterRequestDto:Email:IsRequired"])
            .EmailAddress().WithMessage(localizer["RegisterRequestDto:Email:Invalid"])
            .MaximumLength(256).WithMessage(localizer["RegisterRequestDto:Email:MaxLength", 256]);

        RuleFor(item => item.PhoneNumber)
            .MaximumLength(32).WithMessage(localizer["RegisterRequestDto:PhoneNumber:MaxLength", 32]);

        RuleFor(item => item.Password)
            .NotEmpty().WithMessage(localizer["RegisterRequestDto:Password:IsRequired"])
            .MinimumLength(options.Value.Password.RequiredLength)
            .WithMessage(localizer["RegisterRequestDto:Password:MinLength", options.Value.Password.RequiredLength])
            .MaximumLength(options.Value.Password.MaxLength)
            .WithMessage(localizer["RegisterRequestDto:Password:MaxLength", options.Value.Password.MaxLength]);

        RuleFor(item => item.ConfirmPassword)
            .Equal(item => item.Password).WithMessage(localizer["RegisterRequestDto:ConfirmPassword:DoesNotMatch"]);
    }
}