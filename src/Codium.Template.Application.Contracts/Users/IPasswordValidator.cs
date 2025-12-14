using Codium.Template.Application.Contracts.Common.Results;
using Codium.Template.Domain.Shared.Users;

namespace Codium.Template.Application.Contracts.Users;

public interface IPasswordValidator
{
    ValidationResult Validate(PasswordOptions passwordOptions, string password);
}