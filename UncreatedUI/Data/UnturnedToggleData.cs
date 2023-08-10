using Steamworks;

namespace Uncreated.Framework.UI.Data;
public class UnturnedToggleData : IUnturnedUIData
{
    public CSteamID Player { get; }
    public UnturnedUI Owner { get; }
    public UnturnedButton ToggleButton { get; }
    public bool? Value { get; set; }
    public UnturnedToggleData(CSteamID playerId, UnturnedButton buttonElement, bool? defaultValue = null) : this(playerId, buttonElement.Owner, buttonElement, defaultValue) { }
    public UnturnedToggleData(CSteamID playerId, UnturnedUI owner, UnturnedButton buttonElement, bool? defaultValue = null)
    {
        Player = playerId;
        Owner = owner;
        ToggleButton = buttonElement;
        Value = defaultValue;
    }
    UnturnedUIElement IUnturnedUIData.Element => ToggleButton;
}
