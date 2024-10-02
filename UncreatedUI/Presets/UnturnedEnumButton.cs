using Cysharp.Threading.Tasks;
using DanielWillett.ReflectionTools;
using Microsoft.Extensions.Logging;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Uncreated.Framework.UI.Data;

namespace Uncreated.Framework.UI.Presets;

/// <summary>
/// Handles when an enum button updates.
/// </summary>
public delegate void ValueUpdated<TEnum>(UnturnedEnumButton<TEnum> button, Player player, TEnum value) where TEnum : unmanaged, Enum;

/// <summary>
/// Converts an enum value to text.
/// </summary>
public delegate string TextFormatter<in TEnum>(TEnum value, Player player) where TEnum : unmanaged, Enum;

/// <summary>
/// A 'enum' button that switches between various values of an enum with an optional <see cref="TextFormatter"/>.
/// </summary>
public class UnturnedEnumButton<TEnum> : IStateElement, ILabeledRightClickableButton, IDisposable where TEnum : unmanaged, Enum
{
    [Ignore] private static readonly TEnum[] EnumValues = (TEnum[])typeof(TEnum).GetEnumValues();
    [Ignore] private readonly TEnum[] _values;
    [Ignore] private int[]? _ignored;
    [Ignore] private readonly int _defaultIndex;
    [Ignore] public event ValueUpdated<TEnum>? OnValueUpdated;

    /// <summary>
    /// Initial value of the enum button.
    /// </summary>
    [Ignore] public TEnum Default { get; set; }

    /// <summary>
    /// If clicking should move the button in the opposite direction.
    /// </summary>
    [Ignore] public bool Reversed { get; set; }

    /// <summary>
    /// Custom converter from enum value to <see cref="string"/>.
    /// </summary>
    [Ignore] public TextFormatter<TEnum>? TextFormatter { get; set; }

    /// <summary>
    /// Arbitrary data storage for third-party usage.
    /// </summary>
    [Ignore] public object? Tag { get; set; }

    /// <summary>
    /// Multiple ignored enum values. Use <see cref="Ignored"/> for ignoring just one.
    /// </summary>
    [Ignore]
    public IEnumerable<TEnum>? ManyIgnored
    {
        get => _ignored == null ? Array.Empty<TEnum>() : _ignored.Select(x => _values[x]);
        set => _ignored = value?.Select(x => Array.IndexOf(_values, x)).Where(x => x >= 0).Distinct().ToArray();
    }

    /// <summary>
    /// A single ignored enum value. Use <see cref="ManyIgnored"/> to ignore multiple values.
    /// </summary>
    [Ignore]
    public TEnum Ignored
    {
        set
        {
            int ind = Array.IndexOf(_values, value);
            if (ind == -1)
                _ignored = Array.Empty<int>();
            else _ignored = new int[] { ind };
        }
    }

    /// <summary>
    /// All available values.
    /// </summary>
    [Ignore] public IReadOnlyList<TEnum> Values { get; }

    /// <summary>
    /// If right click goes in the opposite direction. Only supported when <see cref="RightClickListener"/> isn't <see langword="null"/>.
    /// </summary>
    [Ignore] public bool RightClickReverses { get; set; }

    /// <summary>
    /// The button used to listen to left clicks.
    /// </summary>
    public UnturnedButton Button { get; }

    /// <summary>
    /// Optional button used to listen to right clicks.
    /// </summary>
    public UnturnedButton? RightClickListener { get; }

    /// <summary>
    /// The label displaying text on the button.
    /// </summary>
    public UnturnedLabel Label { get; }

    /// <summary>
    /// Optional activation hook object to enable and disable interaction with the button.
    /// </summary>
    public UnturnedUIElement? State { get; }

    /// <summary>
    /// Event fired when <see cref="Button"/> is left clicked.
    /// </summary>
    public event ButtonClicked? OnClicked;

    /// <summary>
    /// Event fired when <see cref="RightClickListener"/> is clicked (<see cref="Button"/> is right clicked).
    /// </summary>
    public event ButtonClicked? OnRightClicked
    {
        add
        {
            if (RightClickListener != null)
                RightClickListener.OnClicked += value;
        }
        remove
        {
            if (RightClickListener != null)
                RightClickListener.OnClicked -= value;
        }
    }

    [Ignore]
    UnturnedButton IRightClickableButton.RightClickListener => RightClickListener!;

    public UnturnedEnumButton(TEnum defaultValue, string buttonPath, string labelPath)
        : this(defaultValue, new UnturnedButton(buttonPath), new UnturnedLabel(UnturnedUIUtility.ResolveRelativePath(buttonPath, labelPath)))
    { }
    public UnturnedEnumButton(TEnum defaultValue, string buttonPath, string labelPath, string? statePath, string? rightClickListenerButtonPath) :
        this(defaultValue, new UnturnedButton(buttonPath), new UnturnedLabel(UnturnedUIUtility.ResolveRelativePath(buttonPath, labelPath)),
            statePath == null ? null : new UnturnedUIElement(UnturnedUIUtility.ResolveRelativePath(buttonPath, statePath)),
            rightClickListenerButtonPath == null ? null : new UnturnedButton(UnturnedUIUtility.ResolveRelativePath(buttonPath, rightClickListenerButtonPath)))
    { }
    public UnturnedEnumButton(TEnum[] valueSet, TEnum defaultValue, string buttonPath, string labelPath) :
        this(valueSet, defaultValue, new UnturnedButton(buttonPath), new UnturnedLabel(UnturnedUIUtility.ResolveRelativePath(buttonPath, labelPath)))
    { }
    public UnturnedEnumButton(TEnum[] valueSet, TEnum defaultValue, string buttonPath, string labelPath, string? statePath, string? rightClickListenerButtonPath) :
        this(valueSet, defaultValue, new UnturnedButton(buttonPath), new UnturnedLabel(UnturnedUIUtility.ResolveRelativePath(buttonPath, labelPath)),
            statePath == null ? null : new UnturnedUIElement(UnturnedUIUtility.ResolveRelativePath(buttonPath, statePath)),
            rightClickListenerButtonPath == null ? null : new UnturnedButton(rightClickListenerButtonPath))
    { }

    public UnturnedEnumButton(TEnum defaultValue, ILabeledButton button)
        : this(defaultValue, button.Button, button.Label, button is IStateElement state ? state.State : null, button is IRightClickableButton rightClickableButton ? rightClickableButton.RightClickListener : null) { }
    public UnturnedEnumButton(TEnum defaultValue, UnturnedButton button, UnturnedLabel label)
        : this(defaultValue, button, label, null, null) { }
    public UnturnedEnumButton(TEnum defaultValue, UnturnedButton button, UnturnedLabel label, UnturnedUIElement? disableState, UnturnedButton? rightClickListener)
        : this(button, label, disableState, rightClickListener)
    {
        _defaultIndex = -1;
        TEnum[] values = EnumValues;
        for (int i = 0; i < values.Length; ++i)
        {
            if (values[i].CompareTo(defaultValue) != 0)
                continue;

            _defaultIndex = i;
            break;
        }

        _values = values;
        Values = new ReadOnlyCollection<TEnum>(values);
    }
    public UnturnedEnumButton(TEnum[] valueSet, TEnum defaultValue, ILabeledButton button)
        : this(valueSet, defaultValue, button.Button, button.Label, button is IStateElement state ? state.State : null, button is IRightClickableButton rightClickableButton ? rightClickableButton.RightClickListener : null) { }
    public UnturnedEnumButton(TEnum[] valueSet, TEnum defaultValue, UnturnedButton button, UnturnedLabel label)
        : this(valueSet, defaultValue, button, label, null, null) { }
    public UnturnedEnumButton(TEnum[] valueSet, TEnum defaultValue, UnturnedButton button, UnturnedLabel label, UnturnedUIElement? disableState, UnturnedButton? rightClickListener)
        : this(button, label, disableState, rightClickListener)
    {
        _defaultIndex = -1;
        for (int i = 0; i < valueSet.Length; ++i)
        {
            if (valueSet[i].CompareTo(defaultValue) != 0)
                continue;

            _defaultIndex = i;
            break;
        }
        _values = valueSet;
        Values = new ReadOnlyCollection<TEnum>(valueSet);
    }

#nullable disable
    private UnturnedEnumButton(UnturnedButton button, UnturnedLabel label, UnturnedUIElement state, UnturnedButton rightClickListener)
    {
        Button = button;
        Label = label;
        State = state;
        RightClickListener = rightClickListener;
        Button.OnClicked += ButtonOnClicked;
        RightClickReverses = RightClickListener != null;
        if (RightClickListener != null)
            RightClickListener!.OnClicked += ButtonOnClicked;
    }
#nullable restore

    /// <summary>
    /// Swich the current value to the default value.
    /// </summary>
    /// <param name="player">Player to switch it for.</param>
    /// <param name="callEvent">Whether or not to invoke the <see cref="OnValueUpdated"/> event.</param>
    public void SetDefault(Player player, bool callEvent = false)
    {
        Set(player, _defaultIndex >= 0 && _defaultIndex < _values.Length ? _values[_defaultIndex] : default, callEvent);
    }

    /// <summary>
    /// Swich the current value to the given <paramref name="value"/>.
    /// </summary>
    /// <param name="player">Player to switch it for.</param>
    /// <param name="value">Enum value to switch to. Will default to <see cref="Default"/> if it's not a valid enum value.</param>
    /// <param name="callEvent">Whether or not to invoke the <see cref="OnValueUpdated"/> event.</param>
    public void Set(Player player, TEnum value, bool callEvent = false)
    {
        int index = Array.IndexOf(_values, value);
        if (index == -1)
            index = _defaultIndex;

        if (Thread.CurrentThread.IsGameThread())
        {
            SetOnMainThread(player, index, callEvent);
        }
        else
        {
            Player p2 = player;
            bool call2 = callEvent;
            int ind2 = index;
            UniTask.Create(async () =>
            {
                await UniTask.SwitchToMainThread();
                SetOnMainThread(p2, ind2, call2);
            });
        }
    }
    private void SetOnMainThread(Player player, int index, bool callEvent)
    {
        UnturnedEnumButtonData? data = UnturnedUIDataSource.Instance.GetData<UnturnedEnumButtonData>(player.channel.owner.playerID.steamID, Button);
        if (data == null)
        {
            data = new UnturnedEnumButtonData(player.channel.owner.playerID.steamID, Button.Owner, this, index);
            UnturnedUIDataSource.Instance.AddData(data);
        }
        else data.SelectionValueIndex = index;

        TEnum? selection = data.Selection;
        if (!selection.HasValue)
            return;
        SendValue(player, selection.Value, callEvent);
    }

    /// <summary>
    /// Update what the player sees from the <see cref="UnturnedEnumButtonData"/> linked to their player.
    /// </summary>
    /// <param name="player">Player to switch it for.</param>
    /// <param name="callEvent">Whether or not to invoke the <see cref="OnValueUpdated"/> event.</param>
    public void Update(Player player, bool callEvent = false)
    {
        if (TryGetSelection(player, out TEnum selection))
            Set(player, selection, callEvent);
        else
            SetDefault(player, callEvent);
    }

    /// <summary>
    /// If it's been set, get the last value the player selected.
    /// </summary>
    public bool TryGetSelection(Player player, out TEnum selection)
    {
        UnturnedEnumButtonData? data = UnturnedUIDataSource.Instance.GetData<UnturnedEnumButtonData>(player.channel.owner.playerID.steamID, Button);
        if (data is not { Selection: not null })
        {
            selection = default;
            return false;
        }

        selection = data.Selection.Value;
        return true;
    }

    /// <summary>
    /// Check if a player has selected anything yet.
    /// </summary>
    public bool HasSelection(Player player)
    {
        UnturnedEnumButtonData? data = UnturnedUIDataSource.Instance.GetData<UnturnedEnumButtonData>(player.channel.owner.playerID.steamID, Button);
        return data is { HasSelection: true };
    }
    private void SendValue(Player player, TEnum value, bool callEvent)
    {
        Label.SetText(player.channel.owner.transportConnection, TextFormatter?.Invoke(value, player) ?? value.ToString());
        if (!callEvent)
            return;

        try
        {
            OnValueUpdated?.Invoke(this, player, value);
        }
        catch (Exception ex)
        {
            Label.Logger.LogError(ex, "[{0}] [{1}] Error invoking {2} for enum button of {3}.", Button.Owner.Name, Button.Name, nameof(OnValueUpdated), typeof(TEnum).Name);
        }
    }

    private void ButtonOnClicked(UnturnedButton button, Player player)
    {
        UnturnedEnumButtonData? data = UnturnedUIDataSource.Instance.GetData<UnturnedEnumButtonData>(player.channel.owner.playerID.steamID, Button);
        if (data == null)
        {
            data = new UnturnedEnumButtonData(player.channel.owner.playerID.steamID, Button.Owner, this, _defaultIndex);
            UnturnedUIDataSource.Instance.AddData(data);
        }

        bool reversed = button == RightClickListener && RightClickReverses;
        int start = Math.Max(0, Math.Min(data.SelectionValueIndex, _values.Length - 1));
        bool found = false;
        if (reversed)
        {
            for (int i = start - 1; i >= 0; --i)
            {
                if (_ignored != null && Array.IndexOf(_ignored, i) != -1)
                    continue;

                data.SelectionValueIndex = i;
                found = true;
                break;
            }

            if (!found)
            {
                for (int i = _values.Length - 1; i >= start; ++i)
                {
                    if (_ignored != null && Array.IndexOf(_ignored, i) != -1)
                        continue;

                    data.SelectionValueIndex = i;
                    found = true;
                    break;
                }
            }
        }
        else
        {
            for (int i = start + 1; i < _values.Length; ++i)
            {
                if (_ignored != null && Array.IndexOf(_ignored, i) != -1)
                    continue;

                data.SelectionValueIndex = i;
                found = true;
                break;
            }

            if (!found)
            {
                for (int i = 0; i <= start; ++i)
                {
                    if (_ignored != null && Array.IndexOf(_ignored, i) != -1)
                        continue;

                    data.SelectionValueIndex = i;
                    found = true;
                    break;
                }
            }
        }

        if (!found)
            return;

        TEnum? selection = data.Selection;
        if (!selection.HasValue)
            return;

        SendValue(player, selection.Value, true);

        if (!reversed)
            OnClicked?.Invoke(button, player);
    }

    /// <summary>
    /// Unsubscribe from the buttons' events.
    /// </summary>
    public void Dispose()
    {
        Button.OnClicked -= ButtonOnClicked;
        if (RightClickListener != null)
            RightClickListener.OnClicked -= ButtonOnClicked;
    }

    [Ignore]
    UnturnedUIElement IElement.Element => Button;

    /// <inheritdoc />
    public override string ToString()
    {
        return typeof(TEnum).Name + " Enum Button [" + Button.Path + "] (" + Button.Owner.Name + ")";
    }

    private class UnturnedEnumButtonData : IUnturnedUIData
    {
        public CSteamID Player { get; }
        public UnturnedUI Owner { get; }
        public UnturnedEnumButton<TEnum> Element { get; }
        UnturnedUIElement IUnturnedUIData.Element => Element.Button;
        public UnturnedEnumButtonData(CSteamID player, UnturnedUI owner, UnturnedEnumButton<TEnum> element, int selectionValueIndex)
        {
            Player = player;
            Owner = owner;
            Element = element;
            SelectionValueIndex = selectionValueIndex;
        }
        public bool HasSelection => SelectionValueIndex >= 0 && SelectionValueIndex < Element._values.Length;
        public int SelectionValueIndex { get; set; }
        public TEnum? Selection => SelectionValueIndex >= 0 && SelectionValueIndex < Element._values.Length ? new TEnum?(Element._values[SelectionValueIndex]) : null;
    }
}
