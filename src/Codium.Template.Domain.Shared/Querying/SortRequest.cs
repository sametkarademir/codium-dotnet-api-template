namespace Codium.Template.Domain.Shared.Querying;

/// <summary>
/// Represents a sort request containing field and order information
/// </summary>
public class SortRequest
{
    public string? Field { get; set; }
    public SortOrderTypes Order { get; set; }

    public SortRequest() : this(null, SortOrderTypes.Desc)
    {
    }

    public SortRequest(string? field, SortOrderTypes order)
    {
        Field = field;
        Order = order;
    }
}

/// <summary>
/// Defines the sort order types for data sorting
/// </summary>
public enum SortOrderTypes
{
    /// <summary>
    /// Ascending sort order
    /// </summary>
    Asc = 0,

    /// <summary>
    /// Descending sort order
    /// </summary>
    Desc = 1
}