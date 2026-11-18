// SPDX-License-Identifier: MIT

using Content.Goobstation.Common._MACRO.Announcements;
using Content.Server.Shuttles.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations;
using Robust.Shared.Utility;

namespace Content.Server.Shuttles.Components;

/// <summary>
/// This is used for controlling evacuation for a station.
/// </summary>
[RegisterComponent]
public sealed partial class StationEmergencyShuttleComponent : Component
{
    /// <summary>
    /// The emergency shuttle assigned to this station.
    /// </summary>
    [DataField, Access(typeof(ShuttleSystem), typeof(EmergencyShuttleSystem), Friend = AccessPermissions.ReadWrite)]
    public EntityUid? EmergencyShuttle;

    /// <summary>
    /// Emergency shuttle map path for this station.
    /// </summary>
    [DataField("emergencyShuttlePath", customTypeSerializer: typeof(ResPathSerializer))]
    public ResPath EmergencyShuttlePath { get; set; } = new("/Maps/Shuttles/emergency.yml");

    // Monkestation edit start - announcer variation
    [DataField]
    public ProtoId<AnnouncementSoundPrototype> ShuttleDockSound = "ShuttleDock";

    [DataField]
    public ProtoId<AnnouncementSoundPrototype> ShuttleNoDockSound = "Notice1";

    [DataField]
    public ProtoId<AnnouncementSoundPrototype> ShuttleGoodLuckSound = "Notice1";
    // Monkestation edit end
}
