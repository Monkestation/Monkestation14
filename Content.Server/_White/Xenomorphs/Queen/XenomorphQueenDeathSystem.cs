using Content.Server.Chat.Systems;
using Content.Shared._White.Xenomorphs.Queen;
using Content.Shared.Mobs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Robust.Shared.Player;
using System.Linq;
using Content.Server._MACRO.Announcements;
using Robust.Shared.Audio;

namespace Content.Server._White.Xenomorphs.Queen;

public sealed class XenomorphQueenDeathSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;

    [Dependency] private readonly AnnouncerManager _announcerManager = default!; // Monkestation edit

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<XenomorphQueenComponent, MobStateChangedEvent>(OnQueenStateChanged);
    }

    private void OnQueenStateChanged(EntityUid uid, XenomorphQueenComponent component, MobStateChangedEvent args)
    {
        // Only proceed if the queen just died
        if (args.NewMobState != MobState.Dead || args.OldMobState == MobState.Dead)
            return;

        // Broadcast to all Xenomorphs
        var filter = Filter.Empty();
        var query = EntityQueryEnumerator<XenomorphComponent, ActorComponent>();
        while (query.MoveNext(out var xenoUid, out _, out var actor))
        {
            if (xenoUid == uid)
                continue; // Skip the queen

            filter.AddPlayer(actor.PlayerSession);
        }

        // Only send if we have players to send to
        // Monkestation edit - announcer variation
            _chat.DispatchFilteredAnnouncement(
                filter,
                "A terrible wail echoes through the tunnels as the Xenomorph Queen falls!",
                component.DeathAnnouncementSound,
                uid,
                "Xenomorph Hivemind",
                true,
                Color.Red);
    }
}
