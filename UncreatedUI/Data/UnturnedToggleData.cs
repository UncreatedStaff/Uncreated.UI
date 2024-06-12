using Steamworks;
using Uncreated.Framework.UI.Presets;

namespace Uncreated.Framework.UI.Data;

/// <summary>
/// Stores the state of a simulated toggle box made of a button and image. See <see cref="UnturnedToggle"/> to use this.
/// </summary>
public class UnturnedToggleData : IUnturnedUIData
{
    /// <inheritdoc />
    public CSteamID Player { get; }

    /// <inheritdoc />
    public UnturnedUI Owner { get; }

    /// <summary>
    /// Button used for the toggle.
    /// </summary>
    public UnturnedButton ToggleButton { get; }

    /// <summary>
    /// Last value of the toggle, if known.
    /// </summary>
    public bool? Value { get; set; }
    public UnturnedToggleData(CSteamID playerId, UnturnedButton buttonElement, bool? defaultValue = null) : this(playerId, buttonElement.Owner, buttonElement, defaultValue) { }
    public UnturnedToggleData(CSteamID playerId, UnturnedUI owner, UnturnedButton buttonElement, bool? defaultValue = null)
    {
        Player = playerId;
        Owner = owner;
        ToggleButton = buttonElement;
        Value = defaultValue;
    }

    /// <inheritdoc />
    UnturnedUIElement IUnturnedUIData.Element => ToggleButton;
}
