namespace Codium.Template.Domain.Shared.Extensions;

public static class EmailTemplateExtensions
{
    private static string LoadTemplate(string templateName)
    {
        var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "EmailTemplates", $"{templateName}.html");
        
        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Email template not found: {templateName}");
        }
        
        return File.ReadAllText(templatePath);
    }
    
    private static string ReplaceTokens(string template, Dictionary<string, string> tokens)
    {
        foreach (var token in tokens)
        {
            template = template.Replace($"{{{{{token.Key}}}}}", token.Value);
        }
        
        return template;
    }
    
    public static string GetEmailConfirmationTemplate(string userName, string confirmationCode, int expiryMinutes, string appName = "Codium")
    {
        var template = LoadTemplate("EmailConfirmation");
        
        var tokens = new Dictionary<string, string>
        {
            { "UserName", userName },
            { "ConfirmationCode", confirmationCode },
            { "ExpiryMinutes", expiryMinutes.ToString() },
            { "AppName", appName },
            { "Year", DateTime.UtcNow.Year.ToString() }
        };
        
        return ReplaceTokens(template, tokens);
    }
    
    public static string GetPasswordResetTemplate(string userName, string resetCode, int expiryMinutes, string appName = "Codium")
    {
        var template = LoadTemplate("PasswordReset");
        
        var tokens = new Dictionary<string, string>
        {
            { "UserName", userName },
            { "ResetCode", resetCode },
            { "ExpiryMinutes", expiryMinutes.ToString() },
            { "AppName", appName },
            { "Year", DateTime.UtcNow.Year.ToString() }
        };
        
        return ReplaceTokens(template, tokens);
    }
}

