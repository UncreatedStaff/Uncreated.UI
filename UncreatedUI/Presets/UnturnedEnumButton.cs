using Steamworks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SDG.NetTransport;
using SDG.Unturned;
using Uncreated.Framework.UI.Data;

namespace Uncreated.Framework.UI.Presets;

public delegate void ValueUpdated<TEnum>(UnturnedEnumButton<TEnum> button, Player player, TEnum value) where TEnum : unmanaged, Enum;
public delegate string TextFormatter<in TEnum>(TEnum value, Player player) where TEnum : unmanaged, Enum;
public class UnturnedEnumButton<TEnum> : IDisposable where TEnum : unmanaged, Enum
{
    [Ignore]
    private static readonly TEnum[] ValuesIntl = (TEnum[])typeof(TEnum).GetEnumValues();
    private int[]? _ignored;
    [Ignore]
    private readonly int _defaultIndex;
    [Ignore]
    public event ValueUpdated<TEnum>? OnValueUpdated;
    public UnturnedButton Button { get; }
    public UnturnedLabel Label { get; }
    public UnturnedUIElement? DisableState { get; }
    [Ignore]
    public TEnum Default { get; set; }
    [Ignore]
    public bool Reversed { get; set; }
    [Ignore]
    public TextFormatter<TEnum>? TextFormatter { get; set; }

    [Ignore]
    public IEnumerable<TEnum>? ManyIgnored
    {
        get => _ignored == null ? Array.Empty<TEnum>() : _ignored.Select(x => ValuesIntl[x]);
        set
        {
            _ignored = value.Select(x => Array.IndexOf(ValuesIntl, x)).Where(x => x >= 0).Distinct().ToArray();
        }
    }

    [Ignore]
    public TEnum Ignored
    {
        set
        {
            int ind = Array.IndexOf(ValuesIntl, value);
            if (ind == -1)
                _ignored = Array.Empty<int>();
            else _ignored = new int[] { ind };
        }
    }

    [Ignore]
    public static IReadOnlyList<TEnum> Values { get; } = new ReadOnlyCollection<TEnum>(ValuesIntl);

    public UnturnedEnumButton(TEnum defaultValue, string button, string label) : this(defaultValue, button, label, null) { }
    public UnturnedEnumButton(TEnum defaultValue, string button, string label, string? disableState)
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
        Button = new UnturnedButton(button);
        Label = new UnturnedLabel(label);
        DisableState = disableState == null ? null : new UnturnedUIElement(disableState);
        Button.OnClicked += ButtonOnClicked;
    }
    private void ButtonOnClicked(UnturnedButton button, Player player)
    {
        UnturnedEnumButtonData? data = UnturnedUIDataSource.Instance.GetData<UnturnedEnumButtonData>(player.channel.owner.playerID.steamID, Button);
        if (data == null)
        {
            data = new UnturnedEnumButtonData(player.channel.owner.playerID.steamID, Button.Owner, Button, _defaultIndex);
            UnturnedUIDataSource.Instance.AddData(data);
        }

        int start = Math.Min(data.SelectionValueIndex, ValuesIntl.Length - 1);
        bool found = false;
        for (int i = start + 1; i < ValuesIntl.Length; ++i)
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

        if (!found)
            return;
        TEnum? selection = data.Selection;
        if (!selection.HasValue)
            return;
        SetIntl(player, selection.Value, true);
    }
    public void SetDefault(Player player, bool callEvent = false) => Set(player, _defaultIndex >= 0 && _defaultIndex < ValuesIntl.Length ? ValuesIntl[_defaultIndex] : default, callEvent);
    public void Set(Player player, TEnum value, bool callEvent = false)
    {
        int index = Array.IndexOf(ValuesIntl, value);
        if (index == -1)
            index = _defaultIndex;
        UnturnedEnumButtonData? data = UnturnedUIDataSource.Instance.GetData<UnturnedEnumButtonData>(player.channel.owner.playerID.steamID, Button);
        if (data == null)
        {
            data = new UnturnedEnumButtonData(player.channel.owner.playerID.steamID, Button.Owner, Button, index);
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
    private void SetIntl(Player player, TEnum value, bool callEvent)
    {
        Label.SetText(player.channel.owner.transportConnection, TextFormatter?.Invoke(value, player) ?? value.ToString());
        if (callEvent)
            OnValueUpdated?.Invoke(this, player, value);
    }
    public void Enable(ITransportConnection player) => DisableState?.SetVisibility(player, true);
    public void Disable(ITransportConnection player) => DisableState?.SetVisibility(player, false);
    public void Show(ITransportConnection player) => Button.SetVisibility(player, true);
    public void Hide(ITransportConnection player) => Button.SetVisibility(player, false);
    public void Dispose()
    {
        Button.OnClicked -= ButtonOnClicked;
    }
    private class UnturnedEnumButtonData : IUnturnedUIData
    {
        public CSteamID Player { get; }
        public UnturnedUI Owner { get; }
        public UnturnedUIElement Element { get; }
        public UnturnedEnumButtonData(CSteamID player, UnturnedUI owner, UnturnedUIElement element, int selectionValueIndex)
        {
            Player = player;
            Owner = owner;
            Element = element;
            SelectionValueIndex = selectionValueIndex;
        }
        public int SelectionValueIndex { get; set; }
        public TEnum? Selection => SelectionValueIndex >= 0 && SelectionValueIndex < ValuesIntl.Length ? new TEnum?(ValuesIntl[SelectionValueIndex]) : null;
    }
}
