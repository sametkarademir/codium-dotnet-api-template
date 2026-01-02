using Codium.Template.Domain.ConfirmationCodes;
using Codium.Template.Domain.Repositories;
using Codium.Template.Domain.Shared.ConfirmationCodes;
using Codium.Template.Domain.Shared.Exceptions.Types;
using Codium.Template.Domain.Shared.Extensions;
using Codium.Template.EntityFrameworkCore.Contexts;
using Codium.Template.EntityFrameworkCore.Repositories.Common;

namespace Codium.Template.EntityFrameworkCore.Repositories;

public class ConfirmationCodeRepository(ApplicationDbContext dbContext)
    : EfRepositoryBase<ConfirmationCode, Guid, ApplicationDbContext>(dbContext), IConfirmationCodeRepository
{
    public async Task<ConfirmationCode> CreateConfirmationCodeAsync(
        Guid userId,
        ConfirmationCodeTypes types,
        int expiryMinutes,
        CancellationToken cancellationToken = default
    )
    {
        var matchedConfirmationCodes = await GetAllAsync(
            predicate: cc =>
                cc.UserId == userId &&
                cc.Type == types &&
                !cc.IsUsed &&
                cc.ExpiryTime > DateTime.UtcNow,
            cancellationToken: cancellationToken
        );

        if (matchedConfirmationCodes.Count != 0)
        {
            var revokedCodes = matchedConfirmationCodes.Select(item =>
            {
                item.IsUsed = true;
                item.UsedTime = DateTime.UtcNow;

                return item;
            }).ToList();
            await UpdateRangeAsync(revokedCodes, cancellationToken);
        }
        
        var newConfirmationCode = new ConfirmationCode
        {
            Id = Guid.NewGuid(),
            Code = CodeGeneratorExtension.Generate6DigitOtp(),
            Type = types,
            ExpiryTime = DateTime.UtcNow.AddMinutes(expiryMinutes),
            IsUsed = false,
            UsedTime = null,
            UserId = userId,
        };
        
        await AddAsync(newConfirmationCode, cancellationToken);
        
        return newConfirmationCode;
    }

    public async Task<ConfirmationCode?> ValidateAndUseConfirmationCodeAsync(
        Guid userId,
        ConfirmationCodeTypes types,
        string code,
        CancellationToken cancellationToken = default)
    {
        var matchedConfirmationCode = await SingleOrDefaultAsync(
            predicate: cc =>
                cc.UserId == userId &&
                cc.Type == types &&
                cc.Code == code &&
                !cc.IsUsed &&
                cc.ExpiryTime > DateTime.UtcNow,
            cancellationToken: cancellationToken
        );

        if (matchedConfirmationCode == null)
        {
            return null;
        }

        matchedConfirmationCode.IsUsed = true;
        matchedConfirmationCode.UsedTime = DateTime.UtcNow;
        await UpdateAsync(matchedConfirmationCode, cancellationToken);
        
        return matchedConfirmationCode;
    }
}