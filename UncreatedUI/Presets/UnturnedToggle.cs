using DanielWillett.ReflectionTools;
using Microsoft.Extensions.Logging;
using SDG.NetTransport;
using SDG.Unturned;
using System;
using System.Threading;
using Uncreated.Framework.UI.Data;

namespace Uncreated.Framework.UI.Presets;

/// <summary>
/// Handles a <see cref="UnturnedToggle"/> being checked or unchecked.
/// </summary>
public delegate void ToggleUpdated(UnturnedToggle toggle, Player player, bool value);

/// <summary>
/// Check box made up of a button and image.
/// </summary>
public class UnturnedToggle : IDisposable, IButton, IStateElement
{
    private int _disposed;

    /// <summary>
    /// Arbitrary data storage for third-party usage.
    /// </summary>
    [Ignore]
    public object? Tag { get; set; }

    /// <summary>
    /// Image of the check mark.
    /// </summary>
    public UnturnedImage ToggleState { get; }

    /// <summary>
    /// The button behind the check mark.
    /// </summary>
    public UnturnedButton ToggleButton { get; }

    /// <summary>
    /// Optional activation object that enables and disables the intractability of the toggle.
    /// </summary>
    public UnturnedUIElement? DisableState { get; }

    /// <summary>
    /// The default value of the toggle.
    /// </summary>
    [Ignore]
    public bool DefaultValue { get; }
    
    /// <summary>
    /// Called when a player checks or unchecks the toggle.
    /// </summary>
    public event ToggleUpdated? OnToggleUpdated;

    /// <inheritdoc />
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

    /// <summary>
    /// Set the current value to the default value.
    /// </summary>
    /// <param name="player">Player to set it for.</param>
    /// <param name="callEvent">Whether or not to invoke the <see cref="OnToggleUpdated"/> event.</param>
    public void SetDefault(Player player, bool callEvent = false) => Set(player, DefaultValue, callEvent);

    /// <summary>
    /// Set the current value to <paramref name="value"/>.
    /// </summary>
    /// <param name="player">Player to set it for.</param>
    /// <param name="value">Wheter or not the toggle is checked.</param>
    /// <param name="callEvent">Whether or not to invoke the <see cref="OnToggleUpdated"/> event.</param>
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

    /// <summary>
    /// Update what the player sees from the <see cref="UnturnedToggleData"/> linked to their player.
    /// </summary>
    /// <param name="player">Player to set it for.</param>
    /// <param name="callEvent">Whether or not to invoke the <see cref="OnToggleUpdated"/> event.</param>
    public void Update(Player player, bool callEvent = false)
    {
        if (TryGetValue(player, out bool value))
            Set(player, value, callEvent);
        else
            SetDefault(player, callEvent);
    }

    /// <summary>
    /// If it's been set, get the last state the player had their toggle at.
    /// </summary>
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

    /// <inheritdoc />
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        ToggleButton.OnClicked -= OnButtonClicked;
    }

    [Ignore]
    UnturnedUIElement IElement.Element => ToggleButton;

    [Ignore]
    UnturnedButton IButton.Button => ToggleButton;

    [Ignore]
    UnturnedUIElement? IStateElement.State => DisableState;
}

/// <summary>
/// Check box made up of a button and image with a label.
/// </summary>
public class LabeledUnturnedToggle : UnturnedToggle, ILabel
{
    /// <inheritdoc />
    public UnturnedLabel Label { get; }
    public LabeledUnturnedToggle(bool defaultValue, string rootButtonName) : this(GlobalLogger.Instance, defaultValue, rootButtonName, rootButtonName + "Label") { }
    public LabeledUnturnedToggle(ILogger logger, bool defaultValue, string rootButtonName) : this(logger, defaultValue, rootButtonName, rootButtonName + "Label") { }
    public LabeledUnturnedToggle(ILoggerFactory factory, bool defaultValue, string rootButtonName) : this(factory, defaultValue, rootButtonName, rootButtonName + "Label") { }
    public LabeledUnturnedToggle(bool defaultValue, string rootButtonName, string labelName) : this(GlobalLogger.Instance, defaultValue, rootButtonName, labelName) { }
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

    /// <summary>
    /// Enable the visibility of the label.
    /// </summary>
    public void ShowLabel(ITransportConnection player) => Label.SetVisibility(player, true);
    
    /// <summary>
    /// Disable the visibility of the label.
    /// </summary>
    public void HideLabel(ITransportConnection player) => Label.SetVisibility(player, false);
}