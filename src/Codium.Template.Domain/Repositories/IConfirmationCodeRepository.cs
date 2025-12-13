using Codium.Template.Domain.ConfirmationCodes;
using Codium.Template.Domain.Shared.Repositories;

namespace Codium.Template.Domain.Repositories;

public interface IConfirmationCodeRepository : IRepository<ConfirmationCode, Guid>
{
}