
using Codium.Template.Domain.Shared.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Codium.Template.Application.Contracts.Users;

public class UpdateUserRequestDto
{
    public string Email { get; set; } = null!;
    
    public bool EmailConfirmed { get; set; }
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }

    public bool TwoFactorEnabled { get; set; }
    public bool LockoutEnabled { get; set; }
    
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateUserRequestDtoValidator : AbstractValidator<UpdateUserRequestDto>
{
    public UpdateUserRequestDtoValidator(IStringLocalizer<ApplicationResource> localizer)
    {
        RuleFor(item => item.Email)
            .NotEmpty().WithMessage(localizer["User:Email:IsRequired"])
            .MaximumLength(256).WithMessage(localizer["User:Email:MaxLength", 256])
            .EmailAddress().WithMessage(localizer["User:Email:InvalidFormat"])
            .When(item => !string.IsNullOrWhiteSpace(item.Email));
        
        RuleFor(item => item.PhoneNumber) 
            .MaximumLength(16).WithMessage(localizer["User:PhoneNumber:MaxLength", 16])
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage(localizer["User:PhoneNumber:InvalidFormat"])
            .When(item => !string.IsNullOrWhiteSpace(item.PhoneNumber));

        RuleFor(item => item.FirstName) 
            .MaximumLength(128).WithMessage(localizer["User:FirstName:MaxLength", 128])
            .When(item => !string.IsNullOrWhiteSpace(item.FirstName));
        
        RuleFor(item => item.LastName) 
            .MaximumLength(128).WithMessage(localizer["User:LastName:MaxLength", 128])
            .When(item => !string.IsNullOrWhiteSpace(item.LastName));
    }
}