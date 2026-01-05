using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Mako.Ranks;

/// <summary>
/// Component that tracks an entity's rank.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class RankComponent : Component
{
    /// <summary>
    /// The ID of the rank prototype assigned to this entity.
    /// </summary>
    [DataField, AutoNetworkedField]
    public ProtoId<RankPrototype>? RankId { get; set; }
}
