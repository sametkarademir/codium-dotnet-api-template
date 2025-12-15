using AutoMapper;
using Codium.Template.Application.BackgroundJobs.InvalidateAllSessions;
using Codium.Template.Application.Contracts.BackgroundJobs;
using Codium.Template.Application.Contracts.BackgroundJobs.InvalidateAllSessions;
using Codium.Template.Application.Contracts.Common.Results;
using Codium.Template.Application.Contracts.Profiles;
using Codium.Template.Application.Contracts.Users;
using Codium.Template.Domain.Repositories;
using Codium.Template.Domain.Shared.Exceptions.Types;
using Codium.Template.Domain.Shared.Extensions;
using Codium.Template.Domain.Shared.Localization;
using Codium.Template.Domain.Shared.Repositories;
using Codium.Template.Domain.Shared.Users;
using Codium.Template.Domain.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace Codium.Template.Application.Profiles;

public class ProfileAppService(
    IUserRepository userRepository,
    ISessionRepository sessionRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher<User> passwordHasher,
    IPasswordValidator passwordValidator,
    IOptions<IdentityUserOptions> options,
    ICurrentUser currentUser,
    IStringLocalizer<ApplicationResource> localizer,
    IMapper mapper,
    IHttpContextAccessor httpContextAccessor,
    IBackgroundJobExecutor backgroundJobExecutor)
    : IProfileAppService
{
    private readonly IdentityUserOptions _identityUserOptions = options.Value;

    public async Task<ProfileResponseDto> GetProfileAsync(CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated)
        {
            throw new AppUnauthorizedException();
        }

        var matchedUser = await userRepository.GetAsync(
            predicate: u => u.Id == currentUser.Id,
            enableTracking: false,
            cancellationToken: cancellationToken
        );

        return new ProfileResponseDto
        {
            Id = matchedUser.Id,
            Email = matchedUser.Email,
            PhoneNumber = matchedUser.PhoneNumber,
            TwoFactorEnabled = matchedUser.TwoFactorEnabled,
            FirstName = matchedUser.FirstName,
            LastName = matchedUser.LastName,
            PasswordChangedTime = matchedUser.PasswordChangedTime,
            Roles = currentUser.Roles ?? [],
            SessionId = currentUser.SessionId!.Value
        };
    }

    public async Task ChangePasswordAsync(ChangePasswordUserRequestDto request, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated)
        {
            throw new AppUnauthorizedException();
        }

        var matchedUser = await userRepository.GetAsync(
            predicate: u => u.Id == currentUser.Id,
            enableTracking: true,
            cancellationToken: cancellationToken
        );
        
        var verificationResult = passwordHasher.VerifyHashedPassword(
            matchedUser,
            matchedUser.PasswordHash,
            request.OldPassword
        );
        
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            throw new AppValidationException(localizer["ProfileAppService:ChangePasswordAsync:InvalidOldPassword"]);
        }
        
        var passwordValidationResult = passwordValidator.Validate(_identityUserOptions.Password, request.NewPassword);
        if (!passwordValidationResult.Succeeded)
        {
            throw new AppValidationException(passwordValidationResult.Errors);
        }
        
        matchedUser.PasswordHash = passwordHasher.HashPassword(matchedUser, request.NewPassword);
        matchedUser.PasswordChangedTime = DateTime.UtcNow;
        
        await userRepository.UpdateAsync(matchedUser, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        backgroundJobExecutor.Enqueue<InvalidateAllSessionsBackgroundJob, InvalidateAllSessionsBackgroundJobArgs>(
            new InvalidateAllSessionsBackgroundJobArgs
            {
                UserId = matchedUser.Id,
                CorrelationId = httpContextAccessor.HttpContext?.GetCorrelationId() ?? Guid.NewGuid(),
                Reason = "Password changed by user"
            }
        );
    }

    public async Task<PagedResult<SessionResponseDto>> GetPageableAndFilterAsync(GetListSessionsRequestDto request, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated)
        {
            throw new AppUnauthorizedException();
        }

        var queryable = sessionRepository.AsQueryable();

        queryable = queryable.Where(s => s.UserId == currentUser.Id);
        queryable = queryable.Where(s => s.IsRevoked == false);
        queryable = queryable.WhereIf(request.IsDesktop != null, s => s.IsDesktop == request.IsDesktop);
        queryable = queryable.WhereIf(request.IsMobile != null, s => s.IsMobile == request.IsMobile);
        queryable = queryable.WhereIf(request.IsTablet != null, s => s.IsTablet == request.IsTablet);

        queryable = queryable.AsNoTracking();
        queryable = queryable.ApplySort(request.Field, request.Order, cancellationToken);
        var pagedSessions = await queryable.ToPageableAsync(request.Page, request.PerPage, cancellationToken);
        
        var mappedSessions = mapper.Map<List<SessionResponseDto>>(pagedSessions.Data);

        return new PagedResult<SessionResponseDto>(mappedSessions, pagedSessions.TotalCount, pagedSessions.Page, pagedSessions.PerPage); 
    }

    public async Task InvalidateSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated)
        {
            throw new AppUnauthorizedException();
        }

        var matchedSession = await sessionRepository.GetAsync(
            predicate: s => 
                s.Id == sessionId && 
                s.UserId == currentUser.Id,
            enableTracking: true,
            cancellationToken: cancellationToken
        );

        matchedSession.IsRevoked = true;
        matchedSession.RevokedTime = DateTime.UtcNow;

        await sessionRepository.UpdateAsync(matchedSession, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var matchedRefreshTokens = await refreshTokenRepository.GetAllAsync(
            predicate: rt => 
                rt.SessionId == matchedSession.Id &&
                rt.UserId == currentUser.Id &&
                rt.IsRevoked == false &&
                rt.IsUsed == false,
            enableTracking: true,
            cancellationToken: cancellationToken
        );

        if (matchedRefreshTokens.Count == 0)
        {
            return;
        }

        var updatedRefreshTokens = matchedRefreshTokens.Select(rt =>
        {
            rt.IsRevoked = true;
            rt.RevokedTime = DateTime.UtcNow;

            return rt;
        }).ToList();

        await refreshTokenRepository.UpdateRangeAsync(updatedRefreshTokens, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}