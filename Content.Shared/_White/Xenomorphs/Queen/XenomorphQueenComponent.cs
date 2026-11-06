using Content.Goobstation.Common._MACRO.Announcements;
using Content.Shared._White.Xenomorphs.Caste;
using Robust.Shared.Prototypes;

namespace Content.Shared._White.Xenomorphs.Queen;

[RegisterComponent]
public sealed partial class XenomorphQueenComponent : Component
{
    [DataField]
    public EntProtoId PromotionActionId = "ActionXenomorphPromotion";

    [DataField]
    public EntProtoId PromoteTo = "MobXenomorphPraetorian";

    [DataField]
    public List<ProtoId<XenomorphCastePrototype>> CasteWhitelist = new() { "Drone", "Hunter", "Sentinel" };

    [DataField]
    public TimeSpan EvolutionDelay = TimeSpan.FromSeconds(3);

    [ViewVariables]
    public EntityUid? PromotionAction;

    // Monkestation edit - announcer variation
    /// <summary>
    /// The sound of the announcement the queen makes when she dies
    /// </summary>
    [DataField]
    public ProtoId<AnnouncementSoundPrototype> DeathAnnouncementSound = "AlienQueenDied";
}
