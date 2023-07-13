using Steamworks;

namespace Uncreated.Framework.UI.Data;
public class UnturnedTextBoxData : IUnturnedUIData
{
    public CSteamID Player { get; }
    public UnturnedUI Owner { get; }
    public UnturnedTextBox TextBox { get; }
    public string? Text { get; set; }
    public UnturnedTextBoxData(CSteamID playerId, UnturnedTextBox element, string? defaultText = null) : this(playerId, element.Owner, element, defaultText) { }
    public UnturnedTextBoxData(CSteamID playerId, UnturnedUI owner, UnturnedTextBox element, string? defaultText = null)
    {
        Player = playerId;
        Owner = owner;
        TextBox = element;
        Text = defaultText;
    }
    UnturnedUIElement IUnturnedUIData.Element => TextBox;
}
