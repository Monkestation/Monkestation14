using Content.Shared._Monkestation.Players;
using Content.Shared.Eui;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Monkestation.Administration;

[Serializable, NetSerializable]
public sealed class RoleTimeExemptionPanelEuiState(
    string playerName,
    RoleTimeExemptionsData? exemptions,
    bool hasAdmin)
    : EuiStateBase
{
    public string PlayerName { get; set; } = playerName;
    public RoleTimeExemptionsData? Exemptions { get; set; } = exemptions;
    public bool HasAdmin { get; set; } = hasAdmin;
}

public static class RoleTimeExemptionPanelEuiStateMsg
{
    [Serializable, NetSerializable]
    public sealed class SetExemptionsRequest(RoleTimeExemptions exemptions) : EuiMessageBase
    {
        public RoleTimeExemptions Exemptions { get; } = exemptions;
    }
}

/// <summary>
///     Contains all the data related to a particular playtime exemption action created by the PlaytimeExemptionPanel window.
/// </summary>
[Serializable, NetSerializable]
public sealed record RoleTimeExemptions(
    List<ProtoId<JobPrototype>> ExemptJobs,
    List<ProtoId<AntagPrototype>> ExemptAntags
);
