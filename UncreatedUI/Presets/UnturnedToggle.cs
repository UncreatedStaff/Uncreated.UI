using DanielWillett.ReflectionTools;
using Microsoft.Extensions.Logging;
using SDG.NetTransport;
using SDG.Unturned;
using System;
using System.Threading;
using Uncreated.Framework.UI.Data;

namespace Uncreated.Framework.UI.Presets;

public delegate void ToggleUpdated(UnturnedToggle toggle, Player player, bool value);
public class UnturnedToggle : IDisposable, IButton, IStateElement
{
    private int _disposed;
    [Ignore]
    public object? Tag { get; set; }

    public UnturnedImage ToggleState { get; }
    public UnturnedButton ToggleButton { get; }
    public UnturnedUIElement? DisableState { get; }

    [Ignore]
    public bool DefaultValue { get; }
    
    public event ToggleUpdated? OnToggleUpdated;

    public event ButtonClicked? OnClicked
    {
        add => ToggleButton.OnClicked += value;
        remove => ToggleButton.OnClicked -= value;
    }

    public UnturnedToggle(bool defaultValue, string rootButtonName) : this(GlobalLogger.Instance, defaultValue, rootButtonName, rootButtonName + "ToggleState") { }
    public UnturnedToggle(ILogger logger, bool defaultValue, string rootButtonName) : this(logger, defaultValue, rootButtonName, rootButtonName + "ToggleState") { }
    public UnturnedToggle(ILoggerFactory factory, bool defaultValue, string rootButtonName) : this(factory, defaultValue, rootButtonName, rootButtonName + "ToggleState") { }
    public UnturnedToggle(bool defaultValue, string rootButtonName, string toggleStateName, string? disableState = null) : this(GlobalLogger.Instance, defaultValue, rootButtonName, toggleStateName, disableState) { }
    public UnturnedToggle(ILogger logger, bool defaultValue, string rootButtonName, string toggleStateName, string? disableState = null)
    {
        ToggleButton = new UnturnedButton(logger, rootButtonName);
        ToggleState = new UnturnedImage(logger, toggleStateName);
        if (disableState != null)
            DisableState = new UnturnedUIElement(logger, disableState);
        DefaultValue = defaultValue;
        ToggleButton.OnClicked += OnButtonClicked;
    }
    public UnturnedToggle(ILoggerFactory factory, bool defaultValue, string rootButtonName, string toggleStateName, string? disableState = null)
    {
        ToggleButton = new UnturnedButton(factory, rootButtonName);
        ToggleState = new UnturnedImage(factory, toggleStateName);
        if (disableState != null)
            DisableState = new UnturnedUIElement(factory, disableState);
        DefaultValue = defaultValue;
        ToggleButton.OnClicked += OnButtonClicked;
    }
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        ToggleButton.OnClicked -= OnButtonClicked;
    }

    private void OnButtonClicked(UnturnedButton button, Player player)
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
        if (!callEvent)
            return;

        try
        {
            OnToggleUpdated?.Invoke(this, player, value);
        }
        catch (Exception ex)
        {
            ToggleButton.Logger.LogError(ex, "[{0}] [{1}] Error invoking {2} for toggle.", ToggleButton.Owner.Name, ToggleButton.Name, nameof(OnToggleUpdated));
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

    [Ignore]
    UnturnedUIElement IElement.Element => ToggleButton;

    [Ignore]
    UnturnedButton IButton.Button => ToggleButton;

    [Ignore]
    UnturnedUIElement? IStateElement.State => DisableState;
}

public class LabeledUnturnedToggle : UnturnedToggle, ILabel
{
    public UnturnedLabel Label { get; }
    public LabeledUnturnedToggle(bool defaultValue, string rootButtonName) : this(GlobalLogger.Instance, defaultValue, rootButtonName, rootButtonName + "Label") { }
    public LabeledUnturnedToggle(ILogger logger, bool defaultValue, string rootButtonName) : this(logger, defaultValue, rootButtonName, rootButtonName + "Label") { }
    public LabeledUnturnedToggle(ILoggerFactory factory, bool defaultValue, string rootButtonName) : this(factory, defaultValue, rootButtonName, rootButtonName + "Label") { }
    public LabeledUnturnedToggle(bool defaultValue, string rootButtonName, string labelName) : this(GlobalLogger.Instance, defaultValue, rootButtonName) { }
    public LabeledUnturnedToggle(ILogger logger, bool defaultValue, string rootButtonName, string labelName) : base(logger, defaultValue, rootButtonName)
    {
        Label = new UnturnedLabel(logger, labelName);
    }
    public LabeledUnturnedToggle(ILoggerFactory factory, bool defaultValue, string rootButtonName, string labelName) : base(factory, defaultValue, rootButtonName)
    {
        Label = new UnturnedLabel(factory, labelName);
    }
    public LabeledUnturnedToggle(bool defaultValue, string rootButtonName, string toggleStateName, string? labelName, string? disableState) : this(GlobalLogger.Instance, defaultValue, rootButtonName, toggleStateName, labelName, disableState) { }
    public LabeledUnturnedToggle(ILogger logger, bool defaultValue, string rootButtonName, string toggleStateName, string? labelName, string? disableState)
        : base(logger, defaultValue, rootButtonName, toggleStateName, disableState)
    {
        Label = new UnturnedLabel(logger, labelName ?? (rootButtonName + "Label"));
    }
    public LabeledUnturnedToggle(ILoggerFactory factory, bool defaultValue, string rootButtonName, string toggleStateName, string? labelName, string? disableState)
        : base(factory, defaultValue, rootButtonName, toggleStateName, disableState)
    {
        Label = new UnturnedLabel(factory, labelName ?? (rootButtonName + "Label"));
    }

    public void ShowLabel(ITransportConnection player) => Label.SetVisibility(player, true);
    public void HideLabel(ITransportConnection player) => Label.SetVisibility(player, false);
}