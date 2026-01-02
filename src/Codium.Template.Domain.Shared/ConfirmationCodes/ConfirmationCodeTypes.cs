namespace Codium.Template.Domain.Shared.ConfirmationCodes;

public enum ConfirmationCodeTypes
{
    EmailConfirmation = 0,
    PhoneConfirmation = 1,
    TwoFactorAuthentication = 2,
    ResetPassword = 3
}