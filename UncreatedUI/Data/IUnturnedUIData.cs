using Steamworks;

namespace Uncreated.Framework.UI.Data;

/// <summary>
/// Stores information linked to a specific player, UI, and element.
/// </summary>
public interface IUnturnedUIData
{
    /// <summary>
    /// The player that the data is linked to.
    /// </summary>
    CSteamID Player { get; }

    /// <summary>
    /// The UI that the data is linked to.
    /// </summary>
    UnturnedUI Owner { get; }

    /// <summary>
    /// The UI element that the data is linked to.
    /// </summary>
    UnturnedUIElement Element { get; }
}