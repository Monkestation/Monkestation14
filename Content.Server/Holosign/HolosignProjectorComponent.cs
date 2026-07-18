// SPDX-License-Identifier: MIT

using Content.Shared.Tag; // Goobstation
using Robust.Shared.Prototypes;

namespace Content.Server.Holosign
{
    [RegisterComponent]
    public sealed partial class HolosignProjectorComponent : Component
    {
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("signProto")]
        public EntProtoId SignProto = "HolosignWetFloor";

        // Monkestation edit - remove charge use

        // Goobstation
        [DataField]
        public ProtoId<TagPrototype> HolosignTag = "Holosign";
    }
}
