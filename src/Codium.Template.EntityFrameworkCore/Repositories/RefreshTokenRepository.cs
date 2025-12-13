using Codium.Template.Domain.RefreshTokens;
using Codium.Template.Domain.Repositories;
using Codium.Template.EntityFrameworkCore.Contexts;
using Codium.Template.EntityFrameworkCore.Repositories.Common;

namespace Codium.Template.EntityFrameworkCore.Repositories;

public class RefreshTokenRepository : EfRepositoryBase<RefreshToken, Guid, ApplicationDbContext>, IRefreshTokenRepository 
{
    public RefreshTokenRepository(ApplicationDbContext context) : base(context)
    {
        
    }
}