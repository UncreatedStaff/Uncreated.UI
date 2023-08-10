using SDG.Unturned;
using System;
using SDG.NetTransport;
using Uncreated.Framework.UI.Data;
using Uncreated.Networking;

namespace Uncreated.Framework.UI.Presets;

public delegate void ToggleUpdated(UnturnedToggle toggle, Player player, bool value);
public class UnturnedToggle : IDisposable
{
    [Ignore]
    public object? Tag { get; set; }

    public UnturnedImage ToggleState { get; }
    public UnturnedButton ToggleButton { get; }
    public UnturnedUIElement? DisableState { get; }

    [Ignore]
    public bool DefaultValue { get; }

    [Ignore]
    public event ToggleUpdated? OnToggleUpdated;

    public UnturnedToggle(bool defaultValue, string rootButtonName) : this (defaultValue, rootButtonName, rootButtonName + "ToggleState") { }
    public UnturnedToggle(bool defaultValue, string rootButtonName, string toggleStateName, string? disableState = null)
    {
        ToggleButton = new UnturnedButton(rootButtonName);
        ToggleState = new UnturnedImage(toggleStateName);
        if (disableState != null)
            DisableState = new UnturnedUIElement(disableState);
        DefaultValue = defaultValue;
        ToggleButton.OnClicked += OnClicked;
    }
    public void Dispose()
    {
        ToggleButton.OnClicked -= OnClicked;
    }

    private void OnClicked(UnturnedButton button, Player player)
    {
        UnturnedToggleData? data = UnturnedUIDataSource.Instance.GetData<UnturnedToggleData>(player.channel.owner.playerID.steamID, ToggleButton);
        if (data == null)
        {
            data = new UnturnedToggleData(player.channel.owner.playerID.steamID, ToggleButton.Owner, ToggleButton, DefaultValue);
            UnturnedUIDataSource.Instance.AddData(data);
        }

        bool value = !data.Value.HasValue || !data.Value.Value;
        data.Value = value;

        SetIntl(player, value, true);
    }

    private void SetIntl(Player player, bool value, bool callEvent)
    {
        ToggleState.SetVisibility(player.channel.owner.transportConnection, value);
        if (callEvent)
        {
            try
            {
                OnToggleUpdated?.Invoke(this, player, value);
            }
            catch (Exception ex)
            {
                Logging.LogError($"[{ToggleButton.Owner.Name}] [{ToggleButton.Name}] Error invoking OnToggleUpdated for toggle.");
                Logging.LogException(ex);
            }
        }
    }
    public void SetDefault(Player player, bool callEvent = false) => Set(player, DefaultValue, callEvent);
    public void Set(Player player, bool value, bool callEvent = false)
    {
        UnturnedToggleData? data = UnturnedUIDataSource.Instance.GetData<UnturnedToggleData>(player.channel.owner.playerID.steamID, ToggleButton);
        if (data == null)
        {
            data = new UnturnedToggleData(player.channel.owner.playerID.steamID, ToggleButton.Owner, ToggleButton, value);
            UnturnedUIDataSource.Instance.AddData(data);
        }
        else data.Value = value;
        
        SetIntl(player, value, callEvent);
    }
    public void Update(Player player, bool callEvent = false)
    {
        if (TryGetValue(player, out bool value))
            Set(player, value, callEvent);
        else
            SetDefault(player, callEvent);
    }
    public bool TryGetValue(Player player, out bool value)
    {
        UnturnedToggleData? data = UnturnedUIDataSource.Instance.GetData<UnturnedToggleData>(player.channel.owner.playerID.steamID, ToggleButton);
        if (data is not { Value: not null })
        {
            value = default;
            return false;
        }

        value = data.Value.Value;
        return true;
    }
    public void Show(ITransportConnection player) => ToggleButton.SetVisibility(player, true);
    public void Hide(ITransportConnection player) => ToggleButton.SetVisibility(player, false);
    public void Enable(ITransportConnection player) => DisableState?.SetVisibility(player, true);
    public void Disable(ITransportConnection player) => DisableState?.SetVisibility(player, false);
}

public class LabeledUnturnedToggle : UnturnedToggle
{
    public UnturnedLabel Label { get; }
    public LabeledUnturnedToggle(bool defaultValue, string rootButtonName) : this(defaultValue, rootButtonName, rootButtonName + "Label") { }
    public LabeledUnturnedToggle(bool defaultValue, string rootButtonName, string labelName) : base(defaultValue, rootButtonName)
    {
        Label = new UnturnedLabel(labelName);
    }
    public LabeledUnturnedToggle(bool defaultValue, string rootButtonName, string toggleStateName, string? labelName, string? disableState)
        : base(defaultValue, rootButtonName, toggleStateName, disableState)
    {
        Label = new UnturnedLabel(labelName ?? (rootButtonName + "Label"));
    }

    public void ShowLabel(ITransportConnection player) => Label.SetVisibility(player, true);
    public void HideLabel(ITransportConnection player) => Label.SetVisibility(player, false);
    public void SetLabelText(ITransportConnection player, string text) => Label.SetText(player, text);
}