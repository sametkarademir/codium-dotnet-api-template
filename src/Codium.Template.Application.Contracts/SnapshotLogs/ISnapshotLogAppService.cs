namespace Codium.Template.Application.Contracts.SnapshotLogs;

public interface ISnapshotLogAppService
{
    Task TakeSnapshotLogAsync();
}