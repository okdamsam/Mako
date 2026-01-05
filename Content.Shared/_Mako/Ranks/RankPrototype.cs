using Robust.Shared.Prototypes;

namespace Content.Shared._Mako.Ranks;

/// <summary>
/// Prototype for a military or organizational rank.
/// </summary>
[Prototype("rank")]
public sealed partial class RankPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// The full name of the rank (e.g., "Ensign", "Lieutenant Commander")
    /// </summary>
    [DataField("name", required: true)]
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// The abbreviated prefix for the rank (e.g., "ENS", "LCDR")
    /// Used as a prefix when the entity speaks in chat
    /// </summary>
    [DataField("prefix", required: true)]
    public string Prefix { get; private set; } = string.Empty;

    /// <summary>
    /// The pay grade of the rank (e.g., "O-1", "E-5", "W-2")
    /// </summary>
    [DataField("grade", required: true)]
    public string Grade { get; private set; } = string.Empty;

    /// <summary>
    /// Numeric pay grade for sorting/comparison purposes
    /// </summary>
    [DataField("payGrade")]
    public int PayGrade { get; private set; } = 0;

    /// <summary>
    /// The category of the rank (e.g., "Enlisted", "Officer", "Warrant")
    /// </summary>
    [DataField("category")]
    public string Category { get; private set; } = "Enlisted";

    /// <summary>
    /// Description of the rank for UI/documentation purposes
    /// </summary>
    [DataField("description")]
    public string Description { get; private set; } = string.Empty;
}
