using Codium.Template.Domain.Shared.BaseEntities.Abstractions;
using Codium.Template.Domain.Shared.ConfirmationCodes;
using Codium.Template.Domain.Users;

namespace Codium.Template.Domain.ConfirmationCodes;

public class ConfirmationCode : FullAuditedEntity<Guid>
{
    public string Code { get; set; } = null!;
    public ConfirmationCodeTypes Type { get; set; }
    public DateTime ExpiryTime { get; set; }

    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }

    public Guid UserId { get; set; }
    public virtual User? User { get; set; }
}