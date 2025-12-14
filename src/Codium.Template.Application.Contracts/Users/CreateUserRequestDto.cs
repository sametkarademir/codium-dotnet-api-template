using Codium.Template.Domain.Shared.Localization;
using Codium.Template.Domain.Shared.Users;
using FluentValidation;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace Codium.Template.Application.Contracts.Users;

public class CreateUserRequestDto
{
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;

    public bool EmailConfirmed { get; set; }
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }

    public bool TwoFactorEnabled { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsActive { get; set; } = true;

    public string Password { get; set; } = null!;
    public string ConfirmPassword { get; set; } = null!;
}

public class CreateUserRequestDtoValidator : AbstractValidator<CreateUserRequestDto>
{
    public CreateUserRequestDtoValidator(IStringLocalizer<ApplicationResource> localizer, IOptions<IdentityUserOptions> options)
    {
        RuleFor(item => item.UserName)
            .NotEmpty().WithMessage(localizer["User:UserName:IsRequired"])
            .MaximumLength(256).WithMessage(localizer["User:UserName:MaxLength", 256]);
        
        RuleFor(item => item.Email)
            .NotEmpty().WithMessage(localizer["User:Email:IsRequired"])
            .MaximumLength(256).WithMessage(localizer["User:Email:MaxLength", 256])
            .EmailAddress().WithMessage(localizer["User:Email:InvalidFormat"])
            .When(item => !string.IsNullOrWhiteSpace(item.Email));
        
        RuleFor(item => item.PhoneNumber) 
            .MaximumLength(16).WithMessage(localizer["User:PhoneNumber:MaxLength", 16])
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage(localizer["User:PhoneNumber:InvalidFormat"])
            .When(item => !string.IsNullOrWhiteSpace(item.PhoneNumber));

        RuleFor(item => item.Password)
            .NotEmpty().WithMessage(localizer["User:Password:IsRequired"])
            .MinimumLength(options.Value.Password.RequiredLength)
            .WithMessage(localizer["User:Password:MinLength", options.Value.Password.RequiredLength])
            .MaximumLength(options.Value.Password.MaxLength)
            .WithMessage(localizer["User:Password:MaxLength", options.Value.Password.MaxLength]);
        
        RuleFor(item => item.ConfirmPassword)
            .Equal(item => item.Password).WithMessage(localizer["User:ConfirmPassword:MustMatchPassword"]);
        
        RuleFor(item => item.FirstName) 
            .MaximumLength(128).WithMessage(localizer["User:FirstName:MaxLength", 128])
            .When(item => !string.IsNullOrWhiteSpace(item.FirstName));
        
        RuleFor(item => item.LastName) 
            .MaximumLength(128).WithMessage(localizer["User:LastName:MaxLength", 128])
            .When(item => !string.IsNullOrWhiteSpace(item.LastName));
    }
}