namespace Codium.Template.Domain.Shared.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, Inherited = true)]
public class DisableAuditLogAttribute : Attribute
{
    
}