using SDG.NetTransport;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Uncreated.Framework.UI.Data;
using Uncreated.Networking;

namespace Uncreated.Framework.UI.Presets;

public delegate void ValueUpdated<TEnum>(UnturnedEnumButton<TEnum> button, Player player, TEnum value) where TEnum : unmanaged, Enum;
public delegate string TextFormatter<in TEnum>(TEnum value, Player player) where TEnum : unmanaged, Enum;
public class UnturnedEnumButton<TEnum> : IDisposable where TEnum : unmanaged, Enum
{
    [Ignore]
    private static TEnum[]? _vals;
    [Ignore]
    private static TEnum[] ValuesIntl => _vals ??= (TEnum[])typeof(TEnum).GetEnumValues();
    [Ignore]
    private int[]? _ignored;
    [Ignore]
    private readonly int _defaultIndex;
    [Ignore]
    public event ValueUpdated<TEnum>? OnValueUpdated;
    [Ignore]
    public TEnum Default { get; set; }
    [Ignore]
    public bool Reversed { get; set; }
    [Ignore]
    public TextFormatter<TEnum>? TextFormatter { get; set; }
    [Ignore]
    public object? Tag { get; set; }

    [Ignore]
    public IEnumerable<TEnum>? ManyIgnored
    {
        get => _ignored == null ? Array.Empty<TEnum>() : _ignored.Select(x => _values[x]);
        set => _ignored = value?.Select(x => Array.IndexOf(_values, x)).Where(x => x >= 0).Distinct().ToArray();
    }

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

    [Ignore]
    public IReadOnlyList<TEnum> Values { get; }
    [Ignore]
    public bool RightClickReverses { get; set; }
    [Ignore]
    private readonly TEnum[] _values;

    public UnturnedButton Button { get; }
    public UnturnedLabel Label { get; }
    public UnturnedUIElement? DisableState { get; }
    public UnturnedButton? RightClickListener { get; }

    public UnturnedEnumButton(TEnum defaultValue, string button, string label) : this(defaultValue, button, label, null, null) { }
    public UnturnedEnumButton(TEnum defaultValue, string button, string label, string? disableState, string? rightClickListener) : this(button, label, disableState, rightClickListener)
    {
        _defaultIndex = -1;
        TEnum[] values = ValuesIntl;
        for (int i = 0; i < values.Length; ++i)
        {
            if (values[i].CompareTo(defaultValue) == 0)
            {
                _defaultIndex = i;
                break;
            }
        }

        _values = values;
        Values = new ReadOnlyCollection<TEnum>(values);
    }
    public UnturnedEnumButton(TEnum[] valueSet, TEnum defaultValue, string button, string label) : this(valueSet, defaultValue, button, label, null, null) { }
    public UnturnedEnumButton(TEnum[] valueSet, TEnum defaultValue, string button, string label, string? disableState, string? rightClickListener) : this(button, label, disableState, rightClickListener)
    {
        _defaultIndex = -1;
        for (int i = 0; i < valueSet.Length; ++i)
        {
            if (valueSet[i].CompareTo(defaultValue) == 0)
            {
                _defaultIndex = i;
                break;
            }
        }
        _values = valueSet;
        Values = new ReadOnlyCollection<TEnum>(valueSet);
    }

#nullable disable
    private UnturnedEnumButton(string button, string label, string disableState, string rightClickListener)
    {
        Button = new UnturnedButton(button);
        Label = new UnturnedLabel(label);
        DisableState = disableState == null ? null : new UnturnedUIElement(disableState);
        RightClickListener = rightClickListener == null ? null : new UnturnedButton(rightClickListener);
        Button.OnClicked += ButtonOnClicked;
        RightClickReverses = RightClickListener != null;
        if (RightClickListener != null)
            RightClickListener!.OnClicked += ButtonOnClicked;
    }
#nullable restore
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
                if (_ignored == null || Array.IndexOf(_ignored, i) == -1)
                {
                    data.SelectionValueIndex = i;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                for (int i = _values.Length - 1; i >= start; ++i)
                {
                    if (_ignored == null || Array.IndexOf(_ignored, i) == -1)
                    {
                        data.SelectionValueIndex = i;
                        found = true;
                        break;
                    }
                }
            }
        }
        else
        {
            for (int i = start + 1; i < _values.Length; ++i)
            {
                if (_ignored == null || Array.IndexOf(_ignored, i) == -1)
                {
                    data.SelectionValueIndex = i;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                for (int i = 0; i <= start; ++i)
                {
                    if (_ignored == null || Array.IndexOf(_ignored, i) == -1)
                    {
                        data.SelectionValueIndex = i;
                        found = true;
                        break;
                    }
                }
            }
        }

        if (!found)
            return;
        TEnum? selection = data.Selection;
        if (!selection.HasValue)
            return;
        SetIntl(player, selection.Value, true);
    }
    public void SetDefault(Player player, bool callEvent = false) => Set(player, _defaultIndex >= 0 && _defaultIndex < _values.Length ? _values[_defaultIndex] : default, callEvent);
    public void Set(Player player, TEnum value, bool callEvent = false)
    {
        int index = Array.IndexOf(_values, value);
        if (index == -1)
            index = _defaultIndex;
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
        SetIntl(player, selection.Value, callEvent);
    }
    public void Update(Player player, bool callEvent = false)
    {
        if (TryGetSelection(player, out TEnum selection))
            Set(player, selection, callEvent);
        else
            SetDefault(player, callEvent);
    }
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
    public bool HasSelection(Player player)
    {
        UnturnedEnumButtonData? data = UnturnedUIDataSource.Instance.GetData<UnturnedEnumButtonData>(player.channel.owner.playerID.steamID, Button);
        return data is { HasSelection: true };
    }
    private void SetIntl(Player player, TEnum value, bool callEvent)
    {
        Label.SetText(player.channel.owner.transportConnection, TextFormatter?.Invoke(value, player) ?? value.ToString());
        if (callEvent)
        {
            try
            {
                OnValueUpdated?.Invoke(this, player, value);
            }
            catch (Exception ex)
            {
                Logging.LogError($"[{Button.Owner.Name}] [{Button.Name}] Error invoking OnValueUpdated for enum button of {typeof(TEnum).Name}.");
                Logging.LogException(ex);
            }
        }
    }
    public void Enable(ITransportConnection player) => DisableState?.SetVisibility(player, true);
    public void Disable(ITransportConnection player) => DisableState?.SetVisibility(player, false);
    public void Show(ITransportConnection player) => Button.SetVisibility(player, true);
    public void Hide(ITransportConnection player) => Button.SetVisibility(player, false);
    public void Dispose()
    {
        Button.OnClicked -= ButtonOnClicked;
        if (RightClickListener != null)
            RightClickListener.OnClicked -= ButtonOnClicked;
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
