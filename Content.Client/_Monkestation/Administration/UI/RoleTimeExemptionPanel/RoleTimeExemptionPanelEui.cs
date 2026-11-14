using Content.Client.Eui;
using Content.Shared._Monkestation.Administration;
using Content.Shared.Eui;
using JetBrains.Annotations;

namespace Content.Client._Monkestation.Administration.UI.RoleTimeExemptionPanel;

[UsedImplicitly]
public sealed class RoleTimeExemptionPanelEui : BaseEui
{
    private readonly RoleTimeExemptionPanel _window;

    public RoleTimeExemptionPanelEui()
    {
        _window = new RoleTimeExemptionPanel();
        _window.OnClose += () => SendMessage(new CloseEuiMessage());
        _window.RoleTimeExemptionSubmitted += exemption =>
            SendMessage(new RoleTimeExemptionPanelEuiStateMsg.SetExemptionsRequest(exemption));
    }

    public override void HandleState(EuiStateBase state)
    {
        if (state is not RoleTimeExemptionPanelEuiState s)
        {
            return;
        }

        _window.UpdatePanelState(s);
    }

    public override void Opened()
    {
        _window.OpenCentered();
    }

    public override void Closed()
    {
        _window.Close();
    }
}
