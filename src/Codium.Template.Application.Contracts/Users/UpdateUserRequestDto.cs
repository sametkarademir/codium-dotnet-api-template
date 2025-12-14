
using Codium.Template.Domain.Shared.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Codium.Template.Application.Contracts.Users;

public class UpdateUserRequestDto
{
    public string? PhoneNumber { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateUserRequestDtoValidator : AbstractValidator<UpdateUserRequestDto>
{
    public UpdateUserRequestDtoValidator(IStringLocalizer<ApplicationResource> localizer)
    {
        RuleFor(item => item.PhoneNumber) 
            .MaximumLength(16).WithMessage(localizer["UpdateUserRequestDto:PhoneNumber:MaxLength", 16])
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage(localizer["UpdateUserRequestDto:PhoneNumber:InvalidFormat"])
            .When(item => !string.IsNullOrWhiteSpace(item.PhoneNumber));

        RuleFor(item => item.FirstName) 
            .MaximumLength(128).WithMessage(localizer["UpdateUserRequestDto:FirstName:MaxLength", 128])
            .When(item => !string.IsNullOrWhiteSpace(item.FirstName));
        
        RuleFor(item => item.LastName) 
            .MaximumLength(128).WithMessage(localizer["UpdateUserRequestDto:LastName:MaxLength", 128])
            .When(item => !string.IsNullOrWhiteSpace(item.LastName));
    }
}