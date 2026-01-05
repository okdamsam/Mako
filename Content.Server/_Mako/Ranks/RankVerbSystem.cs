using System.Linq;
using Content.Server.Administration.Managers;
using Content.Shared._Mako.Ranks;
using Content.Shared.Administration;
using Content.Shared.Database;
using Content.Shared.Verbs;
using Robust.Server.GameObjects;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server._Mako.Ranks;

/// <summary>
/// Provides admin verbs for managing entity ranks via right-click menu.
/// </summary>
public sealed class RankVerbSystem : EntitySystem
{
    [Dependency] private readonly IAdminManager _adminManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly RankSystem _rankSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RankComponent, GetVerbsEvent<Verb>>(OnGetVerbs);
    }

    private void OnGetVerbs(EntityUid uid, RankComponent component, GetVerbsEvent<Verb> args)
    {
        if (!TryComp<ActorComponent>(args.User, out var actor))
            return;

        var player = actor.PlayerSession;
        if (!_adminManager.HasAdminFlag(player, AdminFlags.Admin))
            return;

        // Get all ranks organized by category
        var ranks = _prototypeManager.EnumeratePrototypes<RankPrototype>()
            .OrderBy(r => r.PayGrade)
            .ToList();

        // Create category for enlisted ranks (E-1 to E-9)
        var enlisted = ranks.Where(r => r.PayGrade >= 1 && r.PayGrade <= 9).ToList();
        if (enlisted.Any())
        {
            var enlistedCategory = new VerbCategory("Enlisted", "/Textures/Interface/VerbIcons/outfit.svg.192dpi.png");
            foreach (var rank in enlisted)
            {
                Verb verb = new()
                {
                    Text = $"{rank.Prefix} - {rank.Name}",
                    Category = enlistedCategory,
                    Message = rank.Grade,
                    Act = () => _rankSystem.SetEntityRank(uid, rank.ID),
                    Impact = LogImpact.Low,
                };
                args.Verbs.Add(verb);
            }
        }

        // Create category for warrant officers (W-1 to W-5)
        var warrants = ranks.Where(r => r.PayGrade >= 10 && r.PayGrade <= 14).ToList();
        if (warrants.Any())
        {
            var warrantCategory = new VerbCategory("Warrant Officers", "/Textures/Interface/VerbIcons/outfit.svg.192dpi.png");
            foreach (var rank in warrants)
            {
                Verb verb = new()
                {
                    Text = $"{rank.Prefix} - {rank.Name}",
                    Category = warrantCategory,
                    Message = rank.Grade,
                    Act = () => _rankSystem.SetEntityRank(uid, rank.ID),
                    Impact = LogImpact.Low,
                };
                args.Verbs.Add(verb);
            }
        }

        // Create category for officers (O-1 to O-10)
        var officers = ranks.Where(r => r.PayGrade >= 15).ToList();
        if (officers.Any())
        {
            var officerCategory = new VerbCategory("Officers", "/Textures/Interface/VerbIcons/outfit.svg.192dpi.png");
            foreach (var rank in officers)
            {
                Verb verb = new()
                {
                    Text = $"{rank.Prefix} - {rank.Name}",
                    Category = officerCategory,
                    Message = rank.Grade,
                    Act = () => _rankSystem.SetEntityRank(uid, rank.ID),
                    Impact = LogImpact.Low,
                };
                args.Verbs.Add(verb);
            }
        }

        // Add verb to remove rank
        if (component.RankId != null)
        {
            Verb removeRankVerb = new()
            {
                Text = "Remove Rank",
                Category = VerbCategory.Admin,
                Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/delete.svg.192dpi.png")),
                Act = () => _rankSystem.RemoveEntityRank(uid),
                Impact = LogImpact.Low,
            };

            args.Verbs.Add(removeRankVerb);
        }
    }
}
