using Content.Goobstation.Common._MACRO.Announcements;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Hastur.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class MassWhisperComponent : Component
{
    // Monkestation edit - announcer variation
    /// <summary>
    /// Announce sound file path
    /// </summary>
    [DataField]
    public ProtoId<AnnouncementSoundPrototype> Sound = "HasturGrowl";

    /// <summary>
    /// Duration of madness
    /// </summary>
    [DataField]
    public float Duration = 10f;
}
