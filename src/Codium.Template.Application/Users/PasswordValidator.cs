using Codium.Template.Application.Contracts.Common.Results;
using Codium.Template.Application.Contracts.Users;
using Codium.Template.Domain.Shared.Exceptions;
using Codium.Template.Domain.Shared.Localization;
using Codium.Template.Domain.Shared.Users;
using Microsoft.Extensions.Localization;

namespace Codium.Template.Application.Users;

public class PasswordValidator : IPasswordValidator
{
    private readonly IStringLocalizer<ApplicationResource> _localizer;

    public PasswordValidator(IStringLocalizer<ApplicationResource> localizer)
    {
        _localizer = localizer;
    }

    public ValidationResult Validate(PasswordOptions options, string password)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        
        var errors = new List<string>();
        
        // Length validation
        if (password.Length < options.RequiredLength)
        {
            errors.Add(_localizer["PasswordValidator:Password:MinLength", options.RequiredLength]);
        }
        
        if (password.Length > options.MaxLength)
        {
            
            errors.Add(_localizer["PasswordValidator:Password:MaxLength", options.MaxLength]);
        }
        
        // Digit validation
        if (options.RequireDigit && !password.Any(char.IsDigit))
        {
            errors.Add(_localizer["PasswordValidator:Password:RequireDigit"]);
        }
        
        // Lowercase validation
        if (options.RequireLowercase && !password.Any(char.IsLower))
        {
            errors.Add(_localizer["PasswordValidator:Password:RequireLowercase"]);
        }
        
        // Uppercase validation
        if (options.RequireUppercase && !password.Any(char.IsUpper))
        {
            errors.Add(_localizer["PasswordValidator:Password:RequireUppercase"]);
        }
        
        // Non-alphanumeric validation
        if (options.RequireNonAlphanumeric && !password.Any(c => !char.IsLetterOrDigit(c)))
        {
            errors.Add(_localizer["PasswordValidator:Password:RequireNonAlphanumeric"]);
        }
        
        // Check for unique characters
        if (options.RequiredUniqueChars > 0 && password.Distinct().Count() < options.RequiredUniqueChars)
        {
            errors.Add(_localizer["PasswordValidator:Password:RequireUniqueChars", options.RequiredUniqueChars]);
        }

        return errors.Count != 0
            ? ValidationResult.Failed(
                new List<ValidationExceptionModel> { 
                    new()
                    {
                        Property = "Password",
                        Errors = errors
                    }
                }
            )
            : ValidationResult.Success;
    }
}