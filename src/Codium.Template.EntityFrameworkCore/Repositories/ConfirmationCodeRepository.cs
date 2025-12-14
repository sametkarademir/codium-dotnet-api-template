using Codium.Template.Domain.ConfirmationCodes;
using Codium.Template.Domain.Repositories;
using Codium.Template.EntityFrameworkCore.Contexts;
using Codium.Template.EntityFrameworkCore.Repositories.Common;

namespace Codium.Template.EntityFrameworkCore.Repositories;

public class ConfirmationCodeRepository(ApplicationDbContext dbContext)
    : EfRepositoryBase<ConfirmationCode, Guid, ApplicationDbContext>(dbContext), IConfirmationCodeRepository;