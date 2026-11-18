using Content.Shared.Roles;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Monkestation.Players;

[Serializable, NetSerializable]
public record RoleTimeExemptionsData(
    List<ProtoId<JobPrototype>> JobExemptions,
    List<ProtoId<AntagPrototype>> AntagExemptions
);
