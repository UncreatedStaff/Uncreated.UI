using Steamworks;

namespace Uncreated.Framework.UI.Data;

/// <summary>
/// Stores the last text entered into a text box. See <see cref="UnturnedTextBox.UseData"/> to enable using this.
/// </summary>
public class UnturnedTextBoxData : IUnturnedUIData
{
    /// <inheritdoc />
    public CSteamID Player { get; }

    /// <inheritdoc />
    public UnturnedUI Owner { get; }

    /// <summary>
    /// The text box the text was entered from.
    /// </summary>
    public UnturnedTextBox TextBox { get; }

    /// <summary>
    /// The text last entered into the text box, if known.
    /// </summary>
    public string? Text { get; set; }
    public UnturnedTextBoxData(CSteamID playerId, UnturnedTextBox element, string? defaultText = null) : this(playerId, element.Owner, element, defaultText) { }
    public UnturnedTextBoxData(CSteamID playerId, UnturnedUI owner, UnturnedTextBox element, string? defaultText = null)
    {
        Player = playerId;
        Owner = owner;
        TextBox = element;
        Text = defaultText;
    }

    /// <inheritdoc />
    UnturnedUIElement IUnturnedUIData.Element => TextBox;
}
