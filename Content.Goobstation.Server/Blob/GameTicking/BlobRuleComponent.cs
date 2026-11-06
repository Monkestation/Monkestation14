// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common._MACRO.Announcements;
using Content.Server.GameTicking.Rules;
using Content.Shared.Mind;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Blob.GameTicking;

[RegisterComponent, Access(typeof(BlobRuleSystem), typeof(BlobCoreSystem), typeof(BlobObserverSystem))]
public sealed partial class BlobRuleComponent : Component
{
    // Monkestation edit start - announcer variation
    [DataField]
    public ProtoId<AnnouncementSoundPrototype> DetectedAudio = "BlobDetected";

    [DataField]
    public ProtoId<AnnouncementSoundPrototype> CriticalAudio = "BlobCritical";
    // Monkestation edit end

    [ViewVariables]
    public List<(EntityUid mindId, MindComponent mind)> Blobs = new(); //BlobRoleComponent

    [ViewVariables]
    public BlobStage Stage = BlobStage.Default;

    [ViewVariables]
    public float Accumulator = 0f;
}


public enum BlobStage : byte
{
    Default,
    Begin,
    Critical,
    TheEnd,
}
