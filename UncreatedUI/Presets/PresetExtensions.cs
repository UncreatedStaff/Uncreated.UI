using SDG.NetTransport;
using SDG.Unturned;

namespace Uncreated.Framework.UI.Presets;

/// <summary>
/// Extensions containing shortcut methods for working with elements.
/// </summary>
public static class PresetExtensions
{
    /// <summary>
    /// Enable the intractability of <paramref name="stateElement"/>.
    /// </summary>
    public static void Enable(this IStateElement stateElement, ITransportConnection connection)
        => stateElement.SetState(connection, true);

    /// <summary>
    /// Disable the intractability of <paramref name="stateElement"/>.
    /// </summary>
    public static void Disable(this IStateElement stateElement, ITransportConnection connection)
        => stateElement.SetState(connection, false);

    /// <summary>
    /// Set the intractability of <paramref name="stateElement"/>.
    /// </summary>
    public static void SetState(this IStateElement stateElement, ITransportConnection connection, bool isEnabled)
        => stateElement.State?.SetVisibility(connection, isEnabled);

    /// <summary>
    /// Enable the intractability of <paramref name="stateElement"/>.
    /// </summary>
    public static void Enable(this IStateElement stateElement, Player player)
        => stateElement.SetState(player, true);

    /// <summary>
    /// Disable the intractability of <paramref name="stateElement"/>.
    /// </summary>
    public static void Disable(this IStateElement stateElement, Player player)
        => stateElement.SetState(player, false);

    /// <summary>
    /// Set the intractability of <paramref name="stateElement"/>.
    /// </summary>
    public static void SetState(this IStateElement stateElement, Player player, bool isEnabled)
        => stateElement.State?.SetVisibility(player, isEnabled);

    /// <summary>
    /// Enable the intractability of <paramref name="stateElement"/>.
    /// </summary>
    public static void Enable(this IStateElement stateElement, SteamPlayer player)
        => stateElement.SetState(player, true);

    /// <summary>
    /// Disable the intractability of <paramref name="stateElement"/>.
    /// </summary>
    public static void Disable(this IStateElement stateElement, SteamPlayer player)
        => stateElement.SetState(player, false);

    /// <summary>
    /// Set the intractability of <paramref name="stateElement"/>.
    /// </summary>
    public static void SetState(this IStateElement stateElement, SteamPlayer player, bool isEnabled)
        => stateElement.State?.SetVisibility(player, isEnabled);

    /// <summary>
    /// Enable the visibility of <paramref name="element"/>.
    /// </summary>
    public static void Show(this UnturnedUIElement element, ITransportConnection connection)
        => element.SetVisibility(connection, true);

    /// <summary>
    /// Enable the visibility of <paramref name="element"/>.
    /// </summary>
    public static void Show(this UnturnedUIElement element, Player player)
        => element.SetVisibility(player, true);

    /// <summary>
    /// Enable the visibility of <paramref name="element"/>.
    /// </summary>
    public static void Show(this UnturnedUIElement element, SteamPlayer player)
        => element.SetVisibility(player, true);

    /// <summary>
    /// Disable the visibility of <paramref name="element"/>.
    /// </summary>
    public static void Hide(this UnturnedUIElement element, ITransportConnection connection)
        => element.SetVisibility(connection, false);

    /// <summary>
    /// Disable the visibility of <paramref name="element"/>.
    /// </summary>
    public static void Hide(this UnturnedUIElement element, Player player)
        => element.SetVisibility(player, false);

    /// <summary>
    /// Disable the visibility of <paramref name="element"/>.
    /// </summary>
    public static void Hide(this UnturnedUIElement element, SteamPlayer player)
        => element.SetVisibility(player, false);

    /// <summary>
    /// Set the visibility of <paramref name="element"/>.
    /// </summary>
    public static void SetVisibility(this IElement element, ITransportConnection connection, bool isEnabled)
        => element.Element?.SetVisibility(connection, isEnabled);

    /// <summary>
    /// Set the visibility of <paramref name="element"/>.
    /// </summary>
    public static void SetVisibility(this IElement element, Player player, bool isEnabled)
        => element.Element?.SetVisibility(player, isEnabled);

    /// <summary>
    /// Set the visibility of <paramref name="element"/>.
    /// </summary>
    public static void SetVisibility(this IElement element, SteamPlayer player, bool isEnabled)
        => element.Element?.SetVisibility(player, isEnabled);

    /// <summary>
    /// Enable the visibility of <paramref name="element"/>.
    /// </summary>
    public static void Show(this IElement element, ITransportConnection connection)
        => element.SetVisibility(connection, true);

    /// <summary>
    /// Enable the visibility of <paramref name="element"/>.
    /// </summary>
    public static void Show(this IElement element, Player player)
        => element.SetVisibility(player, true);

    /// <summary>
    /// Enable the visibility of <paramref name="element"/>.
    /// </summary>
    public static void Show(this IElement element, SteamPlayer player)
        => element.SetVisibility(player, true);

    /// <summary>
    /// Disable the visibility of <paramref name="element"/>.
    /// </summary>
    public static void Hide(this IElement element, ITransportConnection connection)
        => element.SetVisibility(connection, false);

    /// <summary>
    /// Disable the visibility of <paramref name="element"/>.
    /// </summary>
    public static void Hide(this IElement element, Player player)
        => element.SetVisibility(player, false);

    /// <summary>
    /// Disable the visibility of <paramref name="element"/>.
    /// </summary>
    public static void Hide(this IElement element, SteamPlayer player)
        => element.SetVisibility(player, false);

    /// <summary>
    /// Set the text of <paramref name="label"/>.
    /// </summary>
    public static void SetText(this ILabel label, ITransportConnection connection, string text)
        => label.Label?.SetText(connection, text);

    /// <summary>
    /// Set the text of <paramref name="label"/>.
    /// </summary>
    public static void SetText(this ILabel label, Player player, string text)
        => label.Label?.SetText(player, text);

    /// <summary>
    /// Set the text of <paramref name="label"/>.
    /// </summary>
    public static void SetText(this ILabel label, SteamPlayer player, string text)
        => label.Label?.SetText(player, text);

    /// <summary>
    /// Set the placeholder text of <paramref name="textBox"/>.
    /// </summary>
    public static void SetPlaceholder(this IPlaceholderTextBox textBox, ITransportConnection connection, string text)
        => textBox.Placeholder?.SetText(connection, text);

    /// <summary>
    /// Set the placeholder text of <paramref name="textBox"/>.
    /// </summary>
    public static void SetPlaceholder(this IPlaceholderTextBox textBox, Player player, string text)
        => textBox.Placeholder?.SetText(player, text);

    /// <summary>
    /// Set the placeholder text of <paramref name="textBox"/>.
    /// </summary>
    public static void SetPlaceholder(this IPlaceholderTextBox textBox, SteamPlayer player, string text)
        => textBox.Placeholder?.SetText(player, text);

    /// <summary>
    /// Set the image of <paramref name="image"/>.
    /// </summary>
    public static void SetImage(this IImage image, ITransportConnection connection, string url, bool forceRefresh = false)
        => image.Image?.SetImage(connection, url, forceRefresh);

    /// <summary>
    /// Set the image of <paramref name="image"/>.
    /// </summary>
    public static void SetImage(this IImage image, Player player, string url, bool forceRefresh = false)
        => image.Image?.SetImage(player, url, forceRefresh);

    /// <summary>
    /// Set the image of <paramref name="image"/>.
    /// </summary>
    public static void SetImage(this IImage image, SteamPlayer player, string url, bool forceRefresh = false)
        => image.Image?.SetImage(player, url, forceRefresh);
}