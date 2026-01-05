using System.Linq;
using Content.Server.IdentityManagement;
using Content.Server.Preferences.Managers;
using Content.Server.Station.Systems;
using Content.Shared._Mako.Ranks;
using Content.Shared.GameTicking;
using Robust.Shared.Prototypes;

namespace Content.Server._Mako.Ranks;

/// <summary>
/// Server-side rank system that handles rank assignment and persistence.
/// </summary>
public sealed class RankSystem : SharedRankSystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IdentitySystem _identity = default!;
    [Dependency] private readonly IServerPreferencesManager _prefsManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
    }

    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent args)
    {
        // Get player preferences to check max rank
        if (!_prefsManager.TryGetCachedPreferences(args.Player.UserId, out var prefs))
        {
            Log.Warning($"Could not get cached preferences for {args.Player.UserId}");
            return;
        }

        var maxRankPayGrade = prefs.MaxRankPayGrade;
        Log.Info($"Player {args.Player.Name} spawning with profile rank: {args.Profile.Rank ?? "null"}, max rank pay grade: {maxRankPayGrade?.ToString() ?? "null"}");
        
        // Check if player has max rank set (not null means they have rank permission)
        if (!maxRankPayGrade.HasValue)
        {
            // No max rank set means no ranks allowed - remove any rank
            if (args.Profile.Rank != null)
            {
                RemoveEntityRank(args.Mob);
                Log.Info($"Removed rank from {ToPrettyString(args.Mob)} - no max rank permission set");
            }
            return;
        }
        
        // Assign rank from profile if available
        if (args.Profile.Rank != null && _prototypeManager.TryIndex<RankPrototype>(args.Profile.Rank, out var profileRank))
        {
            // Check if the character's rank exceeds the player's max rank
            if (profileRank.PayGrade > maxRankPayGrade.Value)
            {
                // Downgrade to the highest available rank at or below max rank
                var allowedRank = GetHighestAllowedRank(maxRankPayGrade.Value);
                if (allowedRank != null)
                {
                    SetEntityRank(args.Mob, allowedRank.ID);
                    Log.Info($"Downgraded {ToPrettyString(args.Mob)} rank from {profileRank.Name} to {allowedRank.Name} due to max rank limit");
                }
                else
                {
                    // No rank available, remove rank
                    RemoveEntityRank(args.Mob);
                    Log.Info($"Removed rank from {ToPrettyString(args.Mob)} - no ranks available at max pay grade {maxRankPayGrade.Value}");
                }
            }
            else
            {
                // Rank is within allowed range
                SetEntityRank(args.Mob, args.Profile.Rank);
                Log.Info($"Assigned rank {profileRank.Name} to {ToPrettyString(args.Mob)}");
            }
        }
        else if (args.Profile.Rank != null)
        {
            Log.Warning($"Player {args.Player.Name} has invalid rank ID: {args.Profile.Rank}");
        }
    }

    /// <summary>
    /// Gets the highest rank prototype at or below the given pay grade.
    /// </summary>
    private RankPrototype? GetHighestAllowedRank(int maxPayGrade)
    {
        return _prototypeManager.EnumeratePrototypes<RankPrototype>()
            .Where(r => r.PayGrade <= maxPayGrade)
            .OrderByDescending(r => r.PayGrade)
            .FirstOrDefault();
    }

    /// <summary>
    /// Sets the rank of an entity.
    /// </summary>
    public void SetEntityRank(EntityUid uid, string rankId)
    {
        if (!_prototypeManager.HasIndex<RankPrototype>(rankId))
        {
            Log.Warning($"Attempted to set invalid rank {rankId} on entity {ToPrettyString(uid)}");
            return;
        }

        var rankComp = EnsureComp<RankComponent>(uid);
        rankComp.RankId = new ProtoId<RankPrototype>(rankId);
        Dirty(uid, rankComp);

        Log.Info($"Set rank component on {ToPrettyString(uid)}: RankId = {rankComp.RankId}");

        // Queue identity update to refresh the entity's name/appearance
        _identity.QueueIdentityUpdate(uid);
    }

    /// <summary>
    /// Removes the rank from an entity.
    /// </summary>
    public void RemoveEntityRank(EntityUid uid)
    {
        if (!TryComp<RankComponent>(uid, out var rankComp))
            return;

        rankComp.RankId = null;
        Dirty(uid, rankComp);

        _identity.QueueIdentityUpdate(uid);
    }
}
