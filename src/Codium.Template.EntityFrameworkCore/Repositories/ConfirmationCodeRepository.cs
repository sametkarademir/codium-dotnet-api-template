using Codium.Template.Domain.ConfirmationCodes;
using Codium.Template.Domain.Repositories;
using Codium.Template.EntityFrameworkCore.Contexts;
using Codium.Template.EntityFrameworkCore.Repositories.Common;

namespace Codium.Template.EntityFrameworkCore.Repositories;

public class ConfirmationCodeRepository : EfRepositoryBase<ConfirmationCode, Guid, ApplicationDbContext>, IConfirmationCodeRepository 
{
    public ConfirmationCodeRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        
    }
}