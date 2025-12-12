using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Codium.Template.Domain.Shared.Extensions;

public static class ExceptionExtensions
{
    /// <summary>
    /// Generates a fingerprint for the exception using SHA256 hashing algorithm.
    /// </summary>
    /// <param name="exception">The exception to generate a fingerprint for.</param>
    /// <returns>A base64 encoded string representing the fingerprint of the exception.</returns>
    /// <remarks>
    /// This method creates a unique fingerprint for the exception based on its type, message, and stack trace.
    /// The stack trace is truncated to a maximum length of 500 characters to ensure consistent fingerprint generation.
    /// </remarks>
    /// <example>
    /// <code>
    /// var exception = new Exception("An error occurred.");
    /// var fingerprint = exception.GenerateFingerprint();
    /// Console.WriteLine(fingerprint);
    /// </code>
    /// </example>
    public static string GenerateFingerprint(this Exception exception)
    {
        using var sha = SHA256.Create();

        var exceptionType = exception.GetType().FullName ?? "UnknownType";
        var message = exception.Message;
        var stackTrace = exception.StackTrace?.Substring(0, Math.Min(500, (exception.StackTrace?.Length ?? 0))) ?? string.Empty;

        var input = $"{exceptionType}|{message}|{stackTrace}";
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha.ComputeHash(bytes);

        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Converts the exception data to a dictionary.
    /// </summary>
    /// <param name="exception">The exception to convert.</param>
    /// <returns>A dictionary containing the exception data.</returns>
    /// <remarks>
    /// This method iterates through the exception's data collection and adds each key-value pair to a dictionary.
    /// The keys are converted to strings, and null values are skipped.
    /// </remarks>
    /// <example>
    /// <code>
    /// var exception = new Exception("An error occurred.");
    /// exception.Data["Key1"] = "Value1";
    /// exception.Data["Key2"] = null;
    /// var data = exception.ConvertExceptionDataToDictionary();
    /// Console.WriteLine(data);
    /// </code>
    /// </example>
    public static Dictionary<string, object> ConvertExceptionDataToDictionary(this Exception exception)
    {
        var data = new Dictionary<string, object>(exception.Data.Count);
        if (exception.Data.Count <= 0)
        {
            return data;
        }

        foreach (var keyObject in exception.Data.Keys)
        {
            var key = keyObject.ToString();
            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            var value = exception.Data[keyObject];
            if (value != null)
            {
                data.Add(key, value);
            }
        }

        return data;
    }

    /// <summary>
    /// Converts the exception data to a JSON string.
    /// </summary>
    /// <param name="exception">The exception to convert.</param>
    /// <returns>A JSON string representing the exception data.</returns>
    /// <remarks>
    /// This method iterates through the exception's data collection and serializes it to a JSON string.
    /// The keys are converted to strings, and null values are skipped.
    /// </remarks>
    /// <example>
    /// <code>
    /// var exception = new Exception("An error occurred.");
    /// exception.Data["Key1"] = "Value1";
    /// exception.Data["Key2"] = null;
    /// var json = exception.ConvertExceptionDataToJson();
    /// Console.WriteLine(json);
    /// </code>
    /// </example>
    public static string? ConvertExceptionDataToJson(this Exception exception)
    {
        try
        {
            var data = ConvertExceptionDataToDictionary(exception);
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            return JsonSerializer.Serialize(data, options);
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Converts the inner exceptions to the exception to a list of dictionaries.
    /// </summary>
    /// <param name="exception">The exception to convert.</param>
    /// <returns>A list of dictionaries representing the inner exceptions.</returns>
    /// <remarks>
    /// This method iterates through the inner exceptions to the exception and adds each inner exception's type, message, and stack trace to a dictionary.
    /// The list contains all inner exceptions in the order they are nested.
    /// </remarks>
    /// <example>
    /// <code>
    /// var innerException = new Exception("Inner exception.");
    /// var outerException = new Exception("Outer exception.", innerException);
    /// var innerExceptionsList = outerException.ConvertInnerExceptionsToList();
    /// Console.WriteLine(innerExceptionsList);
    /// </code>
    /// </example>
    public static List<Dictionary<string, string>> ConvertInnerExceptionsToList(this Exception exception)
    {
        var innerExceptionsList = new List<Dictionary<string, string>>();
        var innerException = exception.InnerException;
        var depth = 0;

        while (innerException != null)
        {
            innerExceptionsList.Add(new Dictionary<string, string>
            {
                { "Type", innerException.GetType().Name },
                { "Message", innerException.Message },
                { "StackTrace", innerException.StackTrace ?? "No stack trace available" },
                { "Depth", depth.ToString() }
            });
            innerException = innerException.InnerException;
            depth++;
        }

        return innerExceptionsList;
    }

    /// <summary>
    /// Converts the inner exceptions to the exception to a JSON string.
    /// </summary>
    /// <param name="exception">The exception to convert.</param>
    /// <returns>A JSON string representing the inner exceptions.</returns>
    /// <remarks>
    /// This method iterates through the inner exceptions to the exception and serializes them to a JSON string.
    /// Each inner exception's type, message, and stack trace are included in the JSON representation.
    /// </remarks>
    /// <example>
    /// <code>
    /// var innerException = new Exception("Inner exception.");
    /// var outerException = new Exception("Outer exception.", innerException);
    /// var json = outerException.ConvertInnerExceptionsToJson();
    /// Console.WriteLine(json);
    /// </code>
    /// </example>
    public static string? ConvertInnerExceptionsToJson(this Exception exception)
    {
        try
        {
            var innerExceptionsList = ConvertInnerExceptionsToList(exception);
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            return JsonSerializer.Serialize(innerExceptionsList, options);
        }
        catch (Exception)
        {
            return null;
        }
    }
}