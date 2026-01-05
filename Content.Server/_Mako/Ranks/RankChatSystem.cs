using Content.Server.Chat.Systems;
using Content.Shared._Mako.Ranks;
using Content.Shared.Chat;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Robust.Shared.Prototypes;

namespace Content.Server._Mako.Ranks;

/// <summary>
/// System that adds rank prefixes to speaker names in chat.
/// </summary>
public sealed class RankChatSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RankComponent, TransformSpeakerNameEvent>(OnTransformSpeakerName);
        SubscribeLocalEvent<RankComponent, InventoryRelayedEvent<TransformSpeakerNameEvent>>(OnInventoryRelay);
    }

    private void OnTransformSpeakerName(EntityUid uid, RankComponent component, ref TransformSpeakerNameEvent args)
    {
        ApplyRankPrefix(uid, component, ref args);
    }

    private void OnInventoryRelay(EntityUid uid, RankComponent component, InventoryRelayedEvent<TransformSpeakerNameEvent> args)
    {
        ApplyRankPrefix(uid, component, ref args.Args);
    }

    private void ApplyRankPrefix(EntityUid uid, RankComponent component, ref TransformSpeakerNameEvent args)
    {
        if (component.RankId == null)
            return;

        if (!_prototypeManager.TryIndex(component.RankId.Value, out RankPrototype? rankProto))
            return;

        // Apply rank prefix to the voice name
        // Format: "RANK. Name" (e.g., "ENS. John Smith")
        args.VoiceName = $"{rankProto.Prefix}. {args.VoiceName}";
    }
}
