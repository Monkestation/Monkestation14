// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Charges.Components;
using Content.Shared.Charges.Systems;
using Content.Shared.Examine;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Holosign;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Physics; // Goobstation
using Content.Shared.Power.Components;
using Content.Shared.Storage;
using Content.Shared.Tag; // Goobstation
using Robust.Shared.Map; // Goobstation
using Robust.Shared.Physics.Components; // Goobstation

namespace Content.Server.Holosign;

public sealed partial class HolosignSystem : EntitySystem
{
    [Dependency] private readonly SharedChargesSystem _charges = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    // Goobstation start
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly TagSystem _tag = default!;

    private EntityQuery<PhysicsComponent> _physicsQuery;
    // Goobstation end

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HolosignProjectorComponent, BeforeRangedInteractEvent>(OnBeforeInteract);

        _physicsQuery = GetEntityQuery<PhysicsComponent>(); // Goobstation
    }

    private void OnBeforeInteract(EntityUid uid, HolosignProjectorComponent component, BeforeRangedInteractEvent args)
    {
        // Goob edit start
        if (args.Handled
            || !args.CanReach) // prevent placing out of range
        // Monkestation edit - remove power cell check
            return;

        // Monkestation edit start - handle charges
        if (TryComp<HolosignSignComponent>(args.Target, out var existingSign) && existingSign.Projector == uid)
        {
            QueueDel(args.Target.Value);
            args.Handled = true;
            return;
        }

        if (HasComp<StorageComponent>(args.Target))
            return;

        if (!_charges.HasCharges(uid, 1))
            return;
        // Monkestation edit end



        // places the holographic sign at the click location, snapped to grid.
        var coords = args.ClickLocation.SnapToGrid(EntityManager);
        var mapCoords = _transform.ToMapCoordinates(coords);
        IEnumerable<EntityUid> look = _map.TryFindGridAt(mapCoords, out var grid, out var gridComp)
            ? _map.GetAnchoredEntities((grid, gridComp), mapCoords)
            : _lookup.GetEntitiesInRange(mapCoords, 0.1f);
        foreach (var entity in look)
        {
            if (_tag.HasTag(entity, component.HolosignTag))
                return;

            if (!_physicsQuery.TryComp(entity, out var physics) || !physics.CanCollide || !physics.Hard) // Goob
                continue;

            if ((physics.CollisionLayer &
                 (int) (CollisionGroup.Impassable |
                        CollisionGroup.LowImpassable |
                        CollisionGroup.MidImpassable |
                        CollisionGroup.HighImpassable)) != 0)
                return;
        }
        if (!_charges.TryUseCharge(uid)) // if no battery or no charge, doesn't work - monkestation edit - convert to charges
            return;
        var holoUid = Spawn(component.SignProto, coords);

        // Monkestation edit start - Add HolosignSignComponent
        var sign = EnsureComp<HolosignSignComponent>(holoUid);
        sign.Projector = uid;
        Dirty(holoUid, sign);
        // Monkestation edit end

        // Goob edit end
        var xform = Transform(holoUid);
        // TODO: Just make the prototype anchored
        if (!xform.Anchored)
            _transform.AnchorEntity(holoUid, xform); // anchor to prevent any tempering with (don't know what could even interact with it)

        args.Handled = true;
    }

    // Monkestation edit start - Add use in hand to clear signs and handle signs ending
    [SubscribeLocalEvent]
    private void OnUseInHand(Entity<HolosignProjectorComponent> ent, ref UseInHandEvent args)
    {
        if (args.Handled)
            return;

        var query = EntityQueryEnumerator<HolosignSignComponent>();

        while (query.MoveNext(out var uid, out var sign))
        {
            if (sign.Projector == ent.Owner)
                QueueDel(uid);
        }

        args.Handled = true;
    }

    [SubscribeLocalEvent]
    private void OnSignTerminating(Entity<HolosignSignComponent> ent, ref EntityTerminatingEvent args)
    {
        var projector = ent.Comp.Projector;
        if (!TryComp<LimitedChargesComponent>(projector, out var charges))
            return;

        _charges.AddCharges((projector.Value, charges, null), 1);
    }
}
