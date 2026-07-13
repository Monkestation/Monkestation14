// SPDX-License-Identifier: MIT

using System.Numerics;
using Content.Goobstation.Common.Footprints;
using Content.Goobstation.Maths.FixedPoint;
using Content.Server.Body.Systems;
using Content.Server.Decals;
using Content.Shared._Monkestation.Nutrition;
using Content.Shared._Monkestation.Nutrition.Components;
using Content.Shared.Actions;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Decals;
using Content.Shared.Fluids.Components;
using Content.Shared.Inventory;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Popups;
using Robust.Shared.Map.Components;
using Robust.Shared.Timing;

namespace Content.Server._Monkestation.Nutrition.EntitySystems;

/// <summary>
/// Makes a slimeperson passively slurp up puddles and clean cleanable dirt off the tiles it
/// walks over (while barefoot), and absorb stains off its own worn clothing and held items
/// (shoes or not, since that soaks straight through the body). Slurped reagents are funneled
/// into the stomach so they metabolize normally (food nourishes, toxins harm), while cleaned
/// dirt grants a flat bit of hunger. The behaviour can be toggled with an innate action.
/// </summary>
public sealed partial class SlimeFloorAbsorptionSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private InventorySystem _inventory = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedSolutionContainerSystem _solution = default!;
    [Dependency] private BodySystem _body = default!;
    [Dependency] private HungerSystem _hunger = default!;
    [Dependency] private SharedMapSystem _map = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private DecalSystem _decals = default!;
    [Dependency] private SharedPopupSystem _popup = default!;

    // Solution that footprint entities keep their tracked reagents in (see FootprintSystem).
    private const string FootprintSolution = "print";

    [SubscribeLocalEvent]
    private void OnMapInit(Entity<SlimeFloorAbsorptionComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent, ref ent.Comp.ToggleActionEntity, ent.Comp.ToggleAction);
        _actions.SetToggled(ent.Comp.ToggleActionEntity, ent.Comp.Enabled);
    }

    [SubscribeLocalEvent]
    private void OnShutdown(Entity<SlimeFloorAbsorptionComponent> ent, ref ComponentShutdown args)
    {
        _actions.RemoveAction(ent.Owner, ent.Comp.ToggleActionEntity);
    }

    [SubscribeLocalEvent]
    private void OnToggle(Entity<SlimeFloorAbsorptionComponent> ent, ref ToggleSlimeFloorAbsorptionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        ent.Comp.Enabled = !ent.Comp.Enabled;
        Dirty(ent);

        _actions.SetToggled(ent.Comp.ToggleActionEntity, ent.Comp.Enabled);
        _popup.PopupEntity(
            Loc.GetString(ent.Comp.Enabled ? "slime-absorption-toggle-on" : "slime-absorption-toggle-off"),
            ent,
            ent);
    }

    [SubscribeLocalEvent]
    private void OnMove(Entity<SlimeFloorAbsorptionComponent> ent, ref MoveEvent args)
    {
        var comp = ent.Comp;
        if (!comp.Enabled)
            return;

        var xform = args.Component;
        if (xform.GridUid is not { } gridUid || !TryComp<MapGridComponent>(gridUid, out var grid))
            return;

        var tile = _map.CoordinatesToTile(gridUid, grid, xform.Coordinates);

        // Only act when we step onto a fresh tile.
        if (comp.LastGrid == gridUid && comp.LastTile == tile)
            return;

        if (_timing.CurTime < comp.NextAbsorb)
            return;

        // Mark the tile only once we're actually going to act, so if we're stepped onto it
        // mid-cooldown it gets retried when the cooldown finishes instead of being skipped.
        comp.LastGrid = gridUid;
        comp.LastTile = tile;

        var didSomething = false;

        // Puddles and floor dirt are cleaned with the feet, so it has to be barefoot for those.
        if (!_inventory.TryGetSlotEntity(ent.Owner, "shoes", out _))
            didSomething |= TryAbsorbTile(ent, gridUid, grid, tile);

        if (didSomething)
            comp.NextAbsorb = _timing.CurTime + comp.Cooldown;
    }

    private bool TryAbsorbTile(Entity<SlimeFloorAbsorptionComponent> ent, EntityUid gridUid, MapGridComponent grid, Vector2i tile)
    {
        var comp = ent.Comp;
        var didSomething = false;

        // --- Puddles: ingest into the stomach so reagents metabolize per their own effects. ---
        var intersecting = _lookup.GetLocalEntitiesIntersecting(gridUid, tile, gridComp: grid);
        foreach (var uid in intersecting)
        {
            // Footprints are thin films, not puddles: slurp the whole mark (stomach permitting) and
            // remove the entity so it visually disappears instead of just fading.
            if (HasComp<FootprintComponent>(uid))
            {
                if (!_solution.TryGetSolution(uid, FootprintSolution, out var printSolution, out var print)
                    || print.Volume <= 0)
                    continue;

                // We want the whole mark anyway, so just let the stomachs sip straight from the
                // print. Leftovers stay in it on their own, no handing anything back.
                var printIngested = IngestIntoStomach(ent.Owner, print);
                if (printIngested <= 0)
                    continue;

                _solution.UpdateChemicals(printSolution.Value);

                // Fully absorbed the mark -> delete it so the prints vanish.
                if (print.Volume <= 0)
                    QueueDel(uid);

                if (comp.NutritionPerVolume > 0)
                    _hunger.ModifyHunger(ent.Owner, printIngested.Float() * comp.NutritionPerVolume);

                didSomething = true;
                continue;
            }

            if (!TryComp<PuddleComponent>(uid, out var puddle))
                continue;

            if (!_solution.ResolveSolution(uid, puddle.SolutionName, ref puddle.Solution, out var puddleSolution)
                || puddleSolution.Volume <= 0)
                continue;

            var amount = FixedPoint2.Min(comp.AbsorbVolume, puddleSolution.Volume);
            if (amount <= 0)
                continue;

            var slurped = _solution.SplitSolution(puddle.Solution.Value, amount);
            var ingested = IngestIntoStomach(ent.Owner, slurped);

            // Hand back whatever the stomachs couldn't hold.
            if (slurped.Volume > 0)
                _solution.TryAddSolution(puddle.Solution.Value, slurped);

            if (ingested <= 0)
                continue;

            // Slurping anything off the floor always nourishes a little.
            if (comp.NutritionPerVolume > 0)
                _hunger.ModifyHunger(ent.Owner, ingested.Float() * comp.NutritionPerVolume);

            didSomething = true;
        }

        // --- Cleanable dirt decals: remove them and gain a flat bit of hunger each. ---
        // Skip when already full: otherwise we'd wipe the dirt and play the slurp sound for no gain.
        if (_hunger.IsHungerBelowState(ent.Owner, HungerThreshold.Overfed)
            && TryComp<DecalGridComponent>(gridUid, out var decalGrid))
        {
            var bounds = _lookup.GetLocalBounds(tile, grid.TileSize).Enlarged(0.5f).Translated(new Vector2(-0.5f, -0.5f));
            foreach (var (index, decal) in _decals.GetDecalsIntersecting(gridUid, bounds, decalGrid))
            {
                if (!decal.Cleanable)
                    continue;

                _decals.RemoveDecal(gridUid, index, decalGrid);
                _hunger.ModifyHunger(ent.Owner, comp.NutritionPerDecal);
                didSomething = true;
            }
        }

        return didSomething;
    }

    /// <summary>
    /// Pours a slurped solution into the slime's stomachs and returns how much they actually took.
    /// Whatever the stomachs couldn't hold is left behind in <paramref name="slurped"/>.
    /// </summary>
    private FixedPoint2 IngestIntoStomach(EntityUid slime, Solution slurped)
    {
        if(!TryComp(slime, out BodyComponent? body))
            return FixedPoint2.Zero;
        var before = slurped.Volume;
        var stomachs = _body.GetBodyOrganEntityComps<StomachComponent>((slime, body));
        stomachs.ForEach(stomach => OnStomachAbsorb(stomach, ref slurped));
        return before - slurped.Volume;
    }

    /// <summary>
    /// Each stomach drinks what it can of the relayed solution, leaving the rest for the caller.
    /// </summary>
    private void OnStomachAbsorb(Entity<StomachComponent> ent, ref Solution solution)
    {
        if (solution.Volume <= 0)
            return;

        if (!_solution.ResolveSolution(ent.Owner, StomachSystem.DefaultSolutionName, ref ent.Comp.Solution, out var stomachSolution)
            || stomachSolution.AvailableVolume <= 0)
            return;

        var take = FixedPoint2.Min(solution.Volume, stomachSolution.AvailableVolume);
        var portion = solution.SplitSolution(take);
        _solution.TryAddSolution(ent.Comp.Solution.Value, portion);
    }
}
