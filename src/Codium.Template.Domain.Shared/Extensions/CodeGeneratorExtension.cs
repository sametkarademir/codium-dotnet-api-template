using System.Security.Cryptography;

namespace Codium.Template.Domain.Shared.Extensions;

public static class CodeGeneratorExtension
{
    public static string Generate6DigitOtp()
    {
        var number = RandomNumberGenerator.GetInt32(100000, 1000000);
        return number.ToString();
    }
}