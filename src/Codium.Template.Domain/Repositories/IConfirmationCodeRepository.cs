using Codium.Template.Domain.ConfirmationCodes;
using Codium.Template.Domain.Shared.ConfirmationCodes;
using Codium.Template.Domain.Shared.Repositories;

namespace Codium.Template.Domain.Repositories;

public interface IConfirmationCodeRepository : IRepository<ConfirmationCode, Guid>
{
    Task<ConfirmationCode> CreateConfirmationCodeAsync(
        Guid userId,
        ConfirmationCodeTypes types,
        int expiryMinutes,
        CancellationToken cancellationToken = default
    );

    Task<ConfirmationCode?> ValidateAndUseConfirmationCodeAsync(
        Guid userId,
        ConfirmationCodeTypes types,
        string code,
        CancellationToken cancellationToken = default
    );
}