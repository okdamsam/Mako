using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Robust.Shared.Prototypes;

namespace Content.Shared._Mako.Ranks;

/// <summary>
/// Shared system for rank functionality that works on both client and server.
/// </summary>
public abstract class SharedRankSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    /// <summary>
    /// Gets the full rank name for an entity, or null if they have no rank.
    /// </summary>
    public string? GetRankName(EntityUid uid)
    {
        if (!TryComp<RankComponent>(uid, out var rank) || rank.RankId == null)
            return null;

        return _prototypeManager.TryIndex(rank.RankId.Value, out RankPrototype? proto)
            ? proto.Name
            : null;
    }

    /// <summary>
    /// Gets the rank prefix for an entity, or null if they have no rank.
    /// </summary>
    public string? GetRankPrefix(EntityUid uid)
    {
        if (!TryComp<RankComponent>(uid, out var rank) || rank.RankId == null)
            return null;

        return _prototypeManager.TryIndex(rank.RankId.Value, out RankPrototype? proto)
            ? proto.Prefix
            : null;
    }

    /// <summary>
    /// Tries to get the rank prototype for an entity.
    /// </summary>
    public bool TryGetRank(EntityUid uid, out RankPrototype? rankProto)
    {
        rankProto = null;

        if (!TryComp<RankComponent>(uid, out var rank) || rank.RankId == null)
            return false;

        return _prototypeManager.TryIndex(rank.RankId.Value, out rankProto);
    }

    /// <summary>
    /// Gets the pay grade for an entity's rank, or null if they have no rank.
    /// </summary>
    public string? GetRankGrade(EntityUid uid)
    {
        if (!TryGetRank(uid, out var proto) || proto == null)
            return null;

        return proto.Grade;
    }
}

/// <summary>
/// System for handling rank examine text.
/// </summary>
public sealed class RankExamineSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RankComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(EntityUid uid, RankComponent component, ExaminedEvent args)
    {
        if (component.RankId == null)
            return;

        if (!_prototypeManager.TryIndex(component.RankId.Value, out RankPrototype? rankProto))
            return;

        var color = GetRankColor(rankProto.PayGrade);
        var coloredRank = $"[color={color}]{rankProto.Name} ({rankProto.Grade})[/color]";

        var identityUid = Identity.Entity(uid, EntityManager);
        var rankText = Loc.GetString("rank-examine-text",
            ("entity", identityUid),
            ("rank", coloredRank),
            ("grade", "")); // Grade is now included in rank, so pass empty string

        args.PushMarkup(rankText);
    }

    /// <summary>
    /// Gets the appropriate color for a rank based on its pay grade.
    /// E1-E3: off-grey, E4-E9: white, O1-O4: gold, O5+: dark gold
    /// </summary>
    private static string GetRankColor(int payGrade)
    {
        // E1-E3: off-grey
        if (payGrade >= 1 && payGrade <= 3)
            return "#C0C0C0";

        // E4-E9: white
        if (payGrade >= 4 && payGrade <= 9)
            return "#FFFFFF";

        // O1-O4: gold
        if (payGrade >= 15 && payGrade <= 18)
            return "#FFD700";

        // O5+: dark gold
        if (payGrade >= 19)
            return "#B8860B";

        // Warrant officers (10-14) and others: default white
        return "#FFFFFF";
    }
}
