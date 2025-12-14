using Codium.Template.Application.Contracts.Auth;
using Codium.Template.Application.Contracts.AuthTokens;
using Codium.Template.Application.Contracts.Users;
using Codium.Template.Domain.ConfirmationCodes;
using Codium.Template.Domain.RefreshTokens;
using Codium.Template.Domain.Repositories;
using Codium.Template.Domain.Sessions;
using Codium.Template.Domain.Shared.ConfirmationCodes;
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
using SignInResult = Codium.Template.Application.Contracts.Common.Results.SignInResult;

namespace Codium.Template.Application.Auth;

public class AuthAppService(
    IUserRepository userRepository,
    IUserRoleRepository userRoleRepository,
    ISessionRepository sessionRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IConfirmationCodeRepository confirmationCodeRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IPasswordValidator passwordValidator,
    IJwtTokenAppService jwtTokenAppService,
    IOptions<IdentityUserOptions> options,
    IPasswordHasher<User> passwordHasher,
    IHttpContextAccessor httpContextAccessor,
    IStringLocalizer<ApplicationResource> localizer
) : IAuthAppService
{
    private readonly IdentityUserOptions _identityUserOptions = options.Value;

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var matchedUser = await userRepository.FindByEmailAsync(request.Email, cancellationToken);
            if (matchedUser == null)
            {
                throw new AppUnauthorizedException(localizer["AuthAppService:LoginAsync:InvalidCredentials"]);
            }

            var signInResult = await CheckPasswordSignInAsync(
                matchedUser,
                request.Password,
                true,
                cancellationToken
            );

            if (signInResult.IsLockedOut)
            {
                throw new AppForbiddenException(localizer["AuthAppService:LoginAsync:LockedOut"]);
            }

            if (!signInResult.Succeeded)
            {
                throw new AppUnauthorizedException(localizer["AuthAppService:LoginAsync:InvalidCredentials"]);
            }
            
            if (!matchedUser.IsActive)
            {
                throw new AppForbiddenException(localizer["AuthAppService:LoginAsync:UserInactive"]);
            }
            
            if (!matchedUser.EmailConfirmed && _identityUserOptions.SignIn.RequireConfirmedEmail)
            {
                throw new AppForbiddenException(localizer["AuthAppService:LoginAsync:EmailNotConfirmed"]);
            }
            
            if (!matchedUser.PhoneNumberConfirmed && _identityUserOptions.SignIn.RequireConfirmedPhoneNumber) 
            {
                throw new AppForbiddenException(localizer["AuthAppService:LoginAsync:PhoneNumberNotConfirmed"]);
            }

            var newUserSessionId = await CreateSessionAsync(matchedUser.Id, cancellationToken);

            var roles = await userRoleRepository
                .AsQueryable()
                .Where(ur => ur.UserId == matchedUser.Id)
                .Select(ur => ur.Role!.Name)
                .ToListAsync(cancellationToken);

            var permissions = await userRoleRepository
                .AsQueryable()
                .Where(ur => ur.UserId == matchedUser.Id)
                .SelectMany(ur => ur.Role!.RolePermissions)
                .Select(rp => rp.Permission!.Name)
                .Distinct()
                .ToListAsync(cancellationToken);

            var tokenResponse = jwtTokenAppService.GenerateJwt(new GenerateJwtTokenRequestDto
            {
                Id = matchedUser.Id,
                Email = matchedUser.Email,
                Roles = roles,
                Permissions = permissions,
                SessionId = newUserSessionId
            });

            await CreateRefreshTokenAsync(
                matchedUser.Id,
                newUserSessionId,
                tokenResponse.RefreshToken,
                null,
                tokenResponse.RefreshTokenExpiryTime,
                cancellationToken
            );

            var response = new LoginResponseDto
            {
                AccessToken = tokenResponse.AccessToken,
                ExpiryTime = tokenResponse.AccessTokenExpiryTime,
                RefreshToken = tokenResponse.RefreshToken
            };

            await transaction.CommitAsync(cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return response;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);

            throw;
        }
    }

    public async Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            if (!currentUser.IsAuthenticated || currentUser.Id == null)
            {
                throw new AppUnauthorizedException();
            }

            var matchedUser = await userRepository.SingleOrDefaultAsync(
                predicate: u => u.Id == currentUser.Id,
                cancellationToken: cancellationToken
            );

            if (matchedUser == null)
            {
                throw new AppUnauthorizedException();
            }

            var matchedRefreshToken = await refreshTokenRepository.FirstOrDefaultAsync(
                predicate: rt =>
                    rt.UserId == matchedUser.Id &&
                    rt.Token == request.RefreshToken &&
                    rt.ExpiryTime > DateTime.UtcNow &&
                    !rt.IsRevoked &&
                    !rt.IsUsed,
                cancellationToken: cancellationToken
            );

            if (matchedRefreshToken == null)
            {
                throw new AppUnauthorizedException();
            }

            matchedRefreshToken.IsUsed = true;
            matchedRefreshToken.RevokedTime = DateTime.UtcNow;
            await refreshTokenRepository.UpdateAsync(matchedRefreshToken, cancellationToken);

            var roles = await userRoleRepository
                .AsQueryable()
                .Where(ur => ur.UserId == matchedUser.Id)
                .Select(ur => ur.Role!.Name)
                .ToListAsync(cancellationToken);

            var permissions = await userRoleRepository
                .AsQueryable()
                .Where(ur => ur.UserId == matchedUser.Id)
                .SelectMany(ur => ur.Role!.RolePermissions)
                .Select(rp => rp.Permission!.Name)
                .Distinct()
                .ToListAsync(cancellationToken);

            var tokenResponse = jwtTokenAppService.GenerateJwt(new GenerateJwtTokenRequestDto
            {
                Id = matchedUser.Id,
                Email = matchedUser.Email,
                Roles = roles,
                Permissions = permissions,
                SessionId = matchedRefreshToken.SessionId
            });

            await CreateRefreshTokenAsync(
                matchedUser.Id,
                matchedRefreshToken.SessionId,
                tokenResponse.RefreshToken,
                matchedRefreshToken.Token,
                tokenResponse.RefreshTokenExpiryTime,
                cancellationToken
            );

            var response = new LoginResponseDto
            {
                AccessToken = tokenResponse.AccessToken,
                ExpiryTime = tokenResponse.AccessTokenExpiryTime,
                RefreshToken = tokenResponse.RefreshToken
            };

            await transaction.CommitAsync(cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return response;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);

            throw;
        }
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            if (!currentUser.IsAuthenticated || currentUser.Id == null)
            {
                throw new AppUnauthorizedException();
            }

            var matchedUser = await userRepository.SingleOrDefaultAsync(
                predicate: u => u.Id == currentUser.Id,
                cancellationToken: cancellationToken
            );

            if (matchedUser == null)
            {
                throw new AppUnauthorizedException();
            }

            if (currentUser.SessionId != null)
            {
                var matchedSession = await sessionRepository.SingleOrDefaultAsync(
                    predicate: s =>
                        s.Id == currentUser.SessionId &&
                        s.UserId == matchedUser.Id,
                    cancellationToken: cancellationToken
                );

                if (matchedSession == null)
                {
                    throw new AppUnauthorizedException();
                }

                if (!matchedSession.IsRevoked)
                {
                    matchedSession.IsRevoked = true;
                    matchedSession.RevokedTime = DateTime.UtcNow;
                    await sessionRepository.UpdateAsync(matchedSession, cancellationToken);
                }

                var matchedRefreshTokens = await refreshTokenRepository.GetAllAsync(
                    predicate: rt =>
                        rt.UserId == matchedUser.Id &&
                        rt.SessionId == matchedSession.Id &&
                        !rt.IsRevoked &&
                        !rt.IsUsed,
                    cancellationToken: cancellationToken
                );

                var revokedTokens = matchedRefreshTokens.Select(item =>
                {
                    item.IsRevoked = true;
                    item.RevokedTime = DateTime.UtcNow;

                    return item;
                }).ToList();

                await refreshTokenRepository.UpdateRangeAsync(revokedTokens, cancellationToken);

                await transaction.CommitAsync(cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);

            throw;
        }
    }

    public async Task RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        var existingUser = await userRepository.ExistsByEmailAsync(request.Email, cancellationToken: cancellationToken);
        if (existingUser)
        {
            throw new AppConflictException(localizer["User:Exception:Exists", request.Email]);
        }
        
        var passwordValidatorResult = passwordValidator.Validate(_identityUserOptions.Password, request.Password);
        if (!passwordValidatorResult.Succeeded)
        {
            throw new AppValidationException(passwordValidatorResult.Errors);
        }
        
        var newUser = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            NormalizedEmail = request.Email.NormalizeValue(),
            EmailConfirmed = false,
            PhoneNumber = request.PhoneNumber,
            PhoneNumberConfirmed = false,
            TwoFactorEnabled = false,
            LockoutEnd = null,
            AccessFailedCount = 0,
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = true
        };
        
        newUser.PasswordHash = passwordHasher.HashPassword(newUser, request.Password);
        newUser.LockoutEnabled = _identityUserOptions.Lockout.AllowedForNewUsers;
        
        await userRepository.AddAsync(newUser, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        if (_identityUserOptions.SignIn.RequireConfirmedEmail)
        {
            await ResendEmailConfirmationLinkAsync(newUser.Email, cancellationToken);
        }
    }

    public async Task ResendEmailConfirmationLinkAsync(string email, CancellationToken cancellationToken = default)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var matchedUser = await userRepository.FindByEmailAsync(email, cancellationToken);
            if (matchedUser == null)
            {
                return;
            }
        
            var matchedConfirmationCodes = await confirmationCodeRepository.GetAllAsync(
                predicate: cc =>
                    cc.UserId == matchedUser.Id &&
                    cc.Type == ConfirmationCodeTypes.EmailConfirmation &&
                    !cc.IsUsed &&
                    cc.ExpiryTime > DateTime.UtcNow,
                cancellationToken: cancellationToken
            );
        
            var revokedCodes = matchedConfirmationCodes.Select(item =>
            {
                item.IsUsed = true;
                item.UsedTime = DateTime.UtcNow;

                return item;
            }).ToList();
            await confirmationCodeRepository.UpdateRangeAsync(revokedCodes, cancellationToken);
        
            var newConfirmationCode = new ConfirmationCode
            {
                Id = Guid.NewGuid(),
                Code = CodeGeneratorExtension.Generate6DigitOtp(),
                Type = ConfirmationCodeTypes.EmailConfirmation,
                ExpiryTime = DateTime.UtcNow.AddMinutes(_identityUserOptions.SignIn.EmailConfirmationCodeExpiryMinutes),
                IsUsed = false,
                UsedTime = null,
                UserId = matchedUser.Id,
            };
            await confirmationCodeRepository.AddAsync(newConfirmationCode, cancellationToken);
            
            await transaction.CommitAsync(cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        
            // TODO: Send confirmation email with the new confirmation code
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync(cancellationToken);

            throw;
        }
    }

    public async Task ConfirmEmailAsync(ConfirmEmailRequestDto request, CancellationToken cancellationToken = default)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var matchedUser = await userRepository.FindByEmailAsync(request.Email, cancellationToken);
            if (matchedUser == null)
            {
                throw new AppUnauthorizedException();
            }

            var matchedConfirmationCode = await confirmationCodeRepository.SingleOrDefaultAsync(
                predicate: cc =>
                    cc.UserId == matchedUser.Id &&
                    cc.Type == ConfirmationCodeTypes.EmailConfirmation &&
                    cc.Code == request.Code &&
                    !cc.IsUsed &&
                    cc.ExpiryTime > DateTime.UtcNow,
                cancellationToken: cancellationToken
            );

            if (matchedConfirmationCode == null)
            {
                throw new AppValidationException(localizer["AuthAppService:ConfirmEmailAsync:InvalidCode"]);
            }

            matchedConfirmationCode.IsUsed = true;
            matchedConfirmationCode.UsedTime = DateTime.UtcNow;
            await confirmationCodeRepository.UpdateAsync(matchedConfirmationCode, cancellationToken);

            matchedUser.EmailConfirmed = true;
            await userRepository.UpdateAsync(matchedUser, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);

            throw;
        }
    }

    public async Task ForgotPasswordAsync(string email, CancellationToken cancellationToken = default)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var matchedUser = await userRepository.FindByEmailAsync(email, cancellationToken);
            if (matchedUser == null)
            {
                return;
            }
            
            var matchedConfirmationCodes = await confirmationCodeRepository.GetAllAsync(
                predicate: cc =>
                    cc.UserId == matchedUser.Id &&
                    cc.Type == ConfirmationCodeTypes.ResetPassword &&
                    !cc.IsUsed &&
                    cc.ExpiryTime > DateTime.UtcNow,
                cancellationToken: cancellationToken
            );
            
            var revokedCodes = matchedConfirmationCodes.Select(item =>
            {
                item.IsUsed = true;
                item.UsedTime = DateTime.UtcNow;

                return item;
            }).ToList();
            
            await confirmationCodeRepository.UpdateRangeAsync(revokedCodes, cancellationToken);
            
            var newConfirmationCode = new ConfirmationCode
            {
                Id = Guid.NewGuid(),
                Code = CodeGeneratorExtension.Generate6DigitOtp(),
                Type = ConfirmationCodeTypes.ResetPassword,
                ExpiryTime = DateTime.UtcNow.AddMinutes(_identityUserOptions.SignIn.ResetPasswordCodeExpiryMinutes),
                IsUsed = false,
                UsedTime = null,
                UserId = matchedUser.Id,
            };
            
            await confirmationCodeRepository.AddAsync(newConfirmationCode, cancellationToken);
            
            await transaction.CommitAsync(cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            
            // TODO: Send confirmation email with the new confirmation code
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync(cancellationToken);

            throw;
        }
    }

    public async Task ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken cancellationToken = default)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var matchedUser = await userRepository.FindByEmailAsync(request.Email, cancellationToken);
            if (matchedUser == null)
            {
                throw new AppUnauthorizedException();
            }

            var matchedConfirmationCode = await confirmationCodeRepository.SingleOrDefaultAsync(
                predicate: cc =>
                    cc.UserId == matchedUser.Id &&
                    cc.Type == ConfirmationCodeTypes.ResetPassword &&
                    cc.Code == request.Code &&
                    !cc.IsUsed &&
                    cc.ExpiryTime > DateTime.UtcNow,
                cancellationToken: cancellationToken
            );

            if (matchedConfirmationCode == null)
            {
                throw new AppValidationException(localizer["AuthAppService:ResetPasswordAsync:InvalidCode"]);
            }

            var passwordValidatorResult = passwordValidator.Validate(_identityUserOptions.Password, request.NewPassword);
            if (!passwordValidatorResult.Succeeded)
            {
                throw new AppValidationException(passwordValidatorResult.Errors);
            }

            matchedConfirmationCode.IsUsed = true;
            matchedConfirmationCode.UsedTime = DateTime.UtcNow;
            await confirmationCodeRepository.UpdateAsync(matchedConfirmationCode, cancellationToken);

            matchedUser.PasswordHash = passwordHasher.HashPassword(matchedUser, request.NewPassword);
            await userRepository.UpdateAsync(matchedUser, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);

            throw;
        }
    }

    private async Task<SignInResult> CheckPasswordSignInAsync(
        User user,
        string password,
        bool lockoutOnFailure,
        CancellationToken cancellationToken
    )
    {
        if (IsLockedOut(user))
        {
            return SignInResult.LockedOut();
        }

        if (VerifyPassword(user, password))
        {
            if (lockoutOnFailure)
            {
                await UnlockAsync(user, cancellationToken);
            }

            return SignInResult.Success();
        }

        if (lockoutOnFailure)
        {
            await AccessFailedAsync(user, cancellationToken);

            if (IsLockedOut(user))
            {
                return SignInResult.LockedOut();
            }
        }

        return SignInResult.Failed();
    }

    private bool IsLockedOut(User user)
    {
        if (!user.LockoutEnabled)
        {
            return false;
        }

        if (user.LockoutEnd == null)
        {
            return false;
        }

        return user.LockoutEnd > DateTimeOffset.UtcNow;
    }

    private bool VerifyPassword(User user, string password)
    {
        var verificationResult = passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            password
        );

        return verificationResult != PasswordVerificationResult.Failed;
    }

    private async Task UnlockAsync(User user, CancellationToken cancellationToken)
    {
        user.LockoutEnd = null;
        user.AccessFailedCount = 0;

        await userRepository.UpdateAsync(user, cancellationToken);
    }

    private async Task AccessFailedAsync(User user, CancellationToken cancellationToken)
    {
        if (!user.LockoutEnabled)
        {
            return;
        }

        user.AccessFailedCount++;

        if (user.AccessFailedCount >= _identityUserOptions.Lockout.MaxFailedAccessAttempts)
        {
            var lockoutMinutes = _identityUserOptions.Lockout.DefaultLockoutTimeSpanMinutes.TotalMinutes;
            user.LockoutEnd = DateTime.UtcNow.AddMinutes(lockoutMinutes);
        }

        await userRepository.UpdateAsync(user, cancellationToken);
    }

    private async Task<Guid> CreateSessionAsync(Guid userId, CancellationToken cancellationToken)
    {
        var matchedActiveSessions = await sessionRepository.GetAllAsync(
            predicate: s =>
                s.UserId == userId &&
                !s.IsRevoked,
            cancellationToken: cancellationToken
        );

        if (matchedActiveSessions.Count >= _identityUserOptions.SignIn.MaxActiveSessionsPerUser)
        {
            var sessionsToRevoke = matchedActiveSessions
                .OrderBy(item => item.CreationTime)
                .Take(matchedActiveSessions.Count - _identityUserOptions.SignIn.MaxActiveSessionsPerUser + 1)
                .ToList();
            var revokedSessions = sessionsToRevoke.Select(item =>
            {
                item.IsRevoked = true;
                item.RevokedTime = DateTime.UtcNow;

                return item;
            }).ToList();
            await sessionRepository.UpdateRangeAsync(revokedSessions, cancellationToken);

            var revokedSessionIds = revokedSessions.Select(item => item.Id).ToList();
            await RevokedRefreshTokenAsync(userId, revokedSessionIds, cancellationToken);
        }

        var deviceInfo = httpContextAccessor.HttpContext?.GetDeviceInfo();
        var clientIp = httpContextAccessor.HttpContext?.GetClientIpAddress() ?? "Unknown";
        var userAgent = httpContextAccessor.HttpContext?.GetUserAgent() ?? "Unknown";
        var snapshotId = httpContextAccessor.HttpContext?.GetSnapshotId();
        var correlationId = httpContextAccessor.HttpContext?.GetCorrelationId();

        var newUserSession = new Session
        {
            Id = Guid.NewGuid(),
            IsRevoked = false,
            ClientIp = clientIp,
            UserAgent = userAgent,
            DeviceFamily = deviceInfo?.DeviceFamily,
            DeviceModel = deviceInfo?.DeviceModel,
            OsFamily = deviceInfo?.OsFamily,
            OsVersion = deviceInfo?.OsVersion,
            BrowserFamily = deviceInfo?.BrowserFamily,
            BrowserVersion = deviceInfo?.BrowserVersion,
            IsMobile = deviceInfo?.IsMobile ?? false,
            IsDesktop = deviceInfo?.IsDesktop ?? false,
            IsTablet = deviceInfo?.IsTablet ?? false,
            SnapshotId = snapshotId,
            CorrelationId = correlationId,
            UserId = userId,
        };
        await sessionRepository.AddAsync(newUserSession, cancellationToken);

        return newUserSession.Id;
    }

    private async Task RevokedRefreshTokenAsync(
        Guid userId,
        List<Guid> revokedSessionIds,
        CancellationToken cancellationToken
    )
    {
        var matchedActiveRefreshTokens = await refreshTokenRepository.GetAllAsync(
            predicate: rt =>
                rt.UserId == userId &&
                revokedSessionIds.Contains(rt.SessionId) &&
                (!rt.IsRevoked || !rt.IsUsed),
            cancellationToken: cancellationToken
        );

        var revokedTokens = matchedActiveRefreshTokens.Select(item =>
        {
            item.IsRevoked = true;
            item.RevokedTime = DateTime.UtcNow;

            return item;
        }).ToList();
        await refreshTokenRepository.UpdateRangeAsync(revokedTokens, cancellationToken);
    }

    private async Task CreateRefreshTokenAsync(
        Guid userId,
        Guid sessionId,
        string refreshToken,
        string? replacedByToken,
        DateTime refreshTokenExpiryTime,
        CancellationToken cancellationToken)
    {
        var newRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = refreshToken,
            ExpiryTime = refreshTokenExpiryTime,
            IsUsed = false,
            IsRevoked = false,
            RevokedTime = null,
            ReplacedByToken = replacedByToken,
            UserId = userId,
            SessionId = sessionId,
        };

        await refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
    }
}