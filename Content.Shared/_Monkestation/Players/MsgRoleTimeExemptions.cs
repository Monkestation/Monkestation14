using Content.Shared.Roles;
using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Monkestation.Players;

/// <summary>
/// Sent server -> client to inform the client of their role time exemptions.
/// </summary>
public sealed class MsgRoleTimeExemptions : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.EntityEvent;

    public List<ProtoId<JobPrototype>> JobExemptions = [];
    public List<ProtoId<AntagPrototype>> AntagExemptions = [];

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        var jobCount = buffer.ReadVariableInt32();
        JobExemptions.EnsureCapacity(jobCount);

        for (var i = 0; i < jobCount; i++)
        {
            JobExemptions.Add(buffer.ReadString());
        }

        var antagCount = buffer.ReadVariableInt32();
        AntagExemptions.EnsureCapacity(antagCount);

        for (var i = 0; i < antagCount; i++)
        {
            AntagExemptions.Add(buffer.ReadString());
        }
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.WriteVariableInt32(JobExemptions.Count);

        foreach (var ban in JobExemptions)
        {
            buffer.Write(ban);
        }

        buffer.WriteVariableInt32(AntagExemptions.Count);

        foreach (var ban in AntagExemptions)
        {
            buffer.Write(ban);
        }
    }

}
