using Codium.Template.Domain.Sessions;
using Codium.Template.Domain.Shared.Repositories;

namespace Codium.Template.Domain.Repositories;

public interface ISessionRepository : IRepository<Session, Guid>
{
    
}