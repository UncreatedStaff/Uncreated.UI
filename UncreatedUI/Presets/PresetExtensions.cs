using SDG.NetTransport;
using SDG.Unturned;

namespace Uncreated.Framework.UI.Presets;
public static class PresetExtensions
{
    public static void Enable(this IStateElement stateElement, ITransportConnection connection)
        => stateElement.SetState(connection, true);
    public static void Disable(this IStateElement stateElement, ITransportConnection connection)
        => stateElement.SetState(connection, false);
    public static void SetState(this IStateElement stateElement, ITransportConnection connection, bool isEnabled)
        => stateElement.State?.SetVisibility(connection, isEnabled);
    public static void Enable(this IStateElement stateElement, Player player)
        => stateElement.SetState(player, true);
    public static void Disable(this IStateElement stateElement, Player player)
        => stateElement.SetState(player, false);
    public static void SetState(this IStateElement stateElement, Player player, bool isEnabled)
        => stateElement.State?.SetVisibility(player, isEnabled);
    public static void Enable(this IStateElement stateElement, SteamPlayer player)
        => stateElement.SetState(player, true);
    public static void Disable(this IStateElement stateElement, SteamPlayer player)
        => stateElement.SetState(player, false);
    public static void SetState(this IStateElement stateElement, SteamPlayer player, bool isEnabled)
        => stateElement.State?.SetVisibility(player, isEnabled);

    public static void Show(this UnturnedUIElement element, ITransportConnection connection)
        => element.SetVisibility(connection, true);
    public static void Show(this UnturnedUIElement element, Player player)
        => element.SetVisibility(player, true);
    public static void Show(this UnturnedUIElement element, SteamPlayer player)
        => element.SetVisibility(player, true);
    public static void Hide(this UnturnedUIElement element, ITransportConnection connection)
        => element.SetVisibility(connection, false);
    public static void Hide(this UnturnedUIElement element, Player player)
        => element.SetVisibility(player, false);
    public static void Hide(this UnturnedUIElement element, SteamPlayer player)
        => element.SetVisibility(player, false);

    public static void SetVisibility(this IElement element, ITransportConnection connection, bool isEnabled)
        => element.Element?.SetVisibility(connection, isEnabled);
    public static void SetVisibility(this IElement element, Player player, bool isEnabled)
        => element.Element?.SetVisibility(player, isEnabled);
    public static void SetVisibility(this IElement element, SteamPlayer player, bool isEnabled)
        => element.Element?.SetVisibility(player, isEnabled);
    public static void Show(this IElement element, ITransportConnection connection)
        => element.SetVisibility(connection, true);
    public static void Show(this IElement element, Player player)
        => element.SetVisibility(player, true);
    public static void Show(this IElement element, SteamPlayer player)
        => element.SetVisibility(player, true);
    public static void Hide(this IElement element, ITransportConnection connection)
        => element.SetVisibility(connection, false);
    public static void Hide(this IElement element, Player player)
        => element.SetVisibility(player, false);
    public static void Hide(this IElement element, SteamPlayer player)
        => element.SetVisibility(player, false);
    
    public static void SetText(this ILabel label, ITransportConnection connection, string text)
        => label.Label?.SetText(connection, text);
    public static void SetText(this ILabel label, Player player, string text)
        => label.Label?.SetText(player, text);
    public static void SetText(this ILabel label, SteamPlayer player, string text)
        => label.Label?.SetText(player, text);
    
    public static void SetPlaceholder(this IPlaceholderTextBox label, ITransportConnection connection, string text)
        => label.Placeholder?.SetText(connection, text);
    public static void SetPlaceholder(this IPlaceholderTextBox label, Player player, string text)
        => label.Placeholder?.SetText(player, text);
    public static void SetPlaceholder(this IPlaceholderTextBox label, SteamPlayer player, string text)
        => label.Placeholder?.SetText(player, text);

    public static void SetImage(this IImage image, ITransportConnection connection, string url, bool forceRefresh = false)
        => image.Image?.SetImage(connection, url, forceRefresh);
    public static void SetImage(this IImage image, Player player, string url, bool forceRefresh = false)
        => image.Image?.SetImage(player, url, forceRefresh);
    public static void SetImage(this IImage image, SteamPlayer player, string url, bool forceRefresh = false)
        => image.Image?.SetImage(player, url, forceRefresh);
}
