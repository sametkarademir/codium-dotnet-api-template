using Codium.Template.Domain.Shared.Localization;
using Codium.Template.Domain.Shared.Users;
using FluentValidation;
using Microsoft.Extensions.Localization;

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
    public RegisterRequestDtoValidator(IStringLocalizer<ApplicationResource> localizer)
    {
        RuleFor(item => item.FirstName)
            .MaximumLength(UserConsts.FirstNameMaxLength).WithMessage(localizer["RegisterRequestDto:FirstName:MaxLength", UserConsts.FirstNameMaxLength]);

        RuleFor(item => item.LastName)
            .MaximumLength(UserConsts.LastNameMaxLength).WithMessage(localizer["RegisterRequestDto:LastName:MaxLength", UserConsts.LastNameMaxLength]);

        RuleFor(item => item.Email)
            .NotEmpty().WithMessage(localizer["RegisterRequestDto:Email:IsRequired"])
            .EmailAddress().WithMessage(localizer["RegisterRequestDto:Email:Invalid"])
            .MaximumLength(UserConsts.EmailMaxLength).WithMessage(localizer["RegisterRequestDto:Email:MaxLength", UserConsts.EmailMaxLength]);

        RuleFor(item => item.PhoneNumber)
            .MaximumLength(UserConsts.PhoneNumberMaxLength).WithMessage(localizer["RegisterRequestDto:PhoneNumber:MaxLength", UserConsts.PhoneNumberMaxLength]);

        RuleFor(item => item.Password)
            .NotEmpty().WithMessage(localizer["RegisterRequestDto:Password:IsRequired"])
            .MinimumLength(UserConsts.PasswordRequiredLength).WithMessage(localizer["RegisterRequestDto:Password:MinLength", UserConsts.PasswordRequiredLength])
            .MaximumLength(UserConsts.PasswordMaxLength).WithMessage(localizer["RegisterRequestDto:Password:MaxLength", UserConsts.PasswordMaxLength]);

        RuleFor(item => item.ConfirmPassword)
            .Equal(item => item.Password).WithMessage(localizer["RegisterRequestDto:ConfirmPassword:DoesNotMatch"]);
    }
}