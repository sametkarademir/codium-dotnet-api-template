using Codium.Template.Domain.Repositories;
using Codium.Template.Domain.Sessions;
using Codium.Template.EntityFrameworkCore.Contexts;
using Codium.Template.EntityFrameworkCore.Repositories.Common;

namespace Codium.Template.EntityFrameworkCore.Repositories;

public class SessionRepository(ApplicationDbContext context)
    : EfRepositoryBase<Session, Guid, ApplicationDbContext>(context), ISessionRepository;