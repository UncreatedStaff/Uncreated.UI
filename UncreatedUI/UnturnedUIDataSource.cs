using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Uncreated.Framework.UI.Data;

namespace Uncreated.Framework.UI;

public interface IUnturnedUIDataSource
{
    IEnumerable<IUnturnedUIData> EnumerateData(CSteamID player);
    IEnumerable<IUnturnedUIData> EnumerateData(UnturnedUI owner);
    IEnumerable<IUnturnedUIData> EnumerateData(UnturnedUIElement element);
    TData? GetData<TData>(CSteamID player, UnturnedUIElement element) where TData : class, IUnturnedUIData;
    void AddData(IUnturnedUIData data);
    void RemovePlayer(CSteamID player);
    void RemoveElement(UnturnedUIElement element);
    void RemoveOwner(UnturnedUI owner);
}

public class UnturnedUIDataSource : IUnturnedUIDataSource, IDisposable
{
    private static IUnturnedUIDataSource _instance = new UnturnedUIDataSource();
    public static IUnturnedUIDataSource Instance
    {
        get => _instance;
        set
        {
            IUnturnedUIDataSource old = Interlocked.Exchange(ref _instance, value);
            if (old is IDisposable disp)
                disp.Dispose();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized, false);
        }
    }

    private bool _disposed;
    private Dictionary<ulong, List<IUnturnedUIData>> _playerData = new Dictionary<ulong, List<IUnturnedUIData>>(96);
    private Dictionary<UnturnedUI, List<IUnturnedUIData>> _ownerData = new Dictionary<UnturnedUI, List<IUnturnedUIData>>(256);
    private Dictionary<UnturnedUIElement, List<IUnturnedUIData>> _elementData = new Dictionary<UnturnedUIElement, List<IUnturnedUIData>>(2048);
    public void AddData(IUnturnedUIData data)
    {
        ThreadUtil.assertIsGameThread();

        if (data.Player.GetEAccountType() == EAccountType.k_EAccountTypeIndividual)
        {
            if (!_playerData.TryGetValue(data.Player.m_SteamID, out List<IUnturnedUIData> list))
                _playerData.Add(data.Player.m_SteamID, list = new List<IUnturnedUIData>(32));
            list.Add(data);
        }
        if (data.Owner != null)
        {
            if (!_ownerData.TryGetValue(data.Owner, out List<IUnturnedUIData> list))
                _ownerData.Add(data.Owner, list = new List<IUnturnedUIData>(256));
            list.Add(data);
        }
        if (data.Element != null)
        {
            if (!_elementData.TryGetValue(data.Element, out List<IUnturnedUIData> list))
                _elementData.Add(data.Element, list = new List<IUnturnedUIData>(96));
            list.Add(data);
        }
    }
    public void RemoveElement(UnturnedUIElement element)
    {
        ThreadUtil.assertIsGameThread();
        
        if (!_elementData.TryGetValue(element, out List<IUnturnedUIData> list))
            return;
        _elementData.Remove(element);
        for (int i = 0; i < list.Count; ++i)
        {
            IUnturnedUIData data = list[i];
            if (_playerData.TryGetValue(data.Player.m_SteamID, out List<IUnturnedUIData> list2))
            {
                list2.Remove(data);
                if (list2.Count == 0)
                    _playerData.Remove(data.Player.m_SteamID);
            }
            if (data.Owner != null && _ownerData.TryGetValue(data.Owner, out list2))
            {
                list2.Remove(data);
                if (list2.Count == 0)
                    _ownerData.Remove(data.Owner);
            }
        }
        list.Clear();
    }
    public void RemoveOwner(UnturnedUI owner)
    {
        ThreadUtil.assertIsGameThread();
        
        if (!_ownerData.TryGetValue(owner, out List<IUnturnedUIData> list))
            return;
        _ownerData.Remove(owner);
        for (int i = 0; i < list.Count; ++i)
        {
            IUnturnedUIData data = list[i];
            if (_playerData.TryGetValue(data.Player.m_SteamID, out List<IUnturnedUIData> list2))
            {
                list2.Remove(data);
                if (list2.Count == 0)
                    _playerData.Remove(data.Player.m_SteamID);
            }
            if (data.Element != null && _elementData.TryGetValue(data.Element, out list2))
            {
                list2.Remove(data);
                if (list2.Count == 0)
                    _elementData.Remove(data.Element);
            }
        }
        list.Clear();
    }
    public void RemovePlayer(CSteamID player)
    {
        ThreadUtil.assertIsGameThread();
        
        if (!_playerData.TryGetValue(player.m_SteamID, out List<IUnturnedUIData> list))
            return;
        _playerData.Remove(player.m_SteamID);
        for (int i = 0; i < list.Count; ++i)
        {
            IUnturnedUIData data = list[i];
            if (data.Owner != null && _ownerData.TryGetValue(data.Owner, out List<IUnturnedUIData> list2))
            {
                list2.Remove(data);
                if (list2.Count == 0)
                    _ownerData.Remove(data.Owner);
            }
            if (data.Element != null && _elementData.TryGetValue(data.Element, out list2))
            {
                list2.Remove(data);
                if (list2.Count == 0)
                    _elementData.Remove(data.Element);
            }
        }
        list.Clear();
    }

    public TData? GetData<TData>(CSteamID player, UnturnedUIElement element) where TData : class, IUnturnedUIData
    {
        ThreadUtil.assertIsGameThread();

        if (_elementData.TryGetValue(element, out List<IUnturnedUIData> data))
        {
            for (int i = data.Count - 1; i >= 0; --i)
            {
                if (data[i] is TData data2 && data2.Player.m_SteamID == player.m_SteamID)
                    return data2;
            }
        }

        return null;
    }

    public IEnumerable<IUnturnedUIData> EnumerateData(CSteamID player)
    {
        ThreadUtil.assertIsGameThread();

        return _playerData.TryGetValue(player.m_SteamID, out List<IUnturnedUIData> data) ? data : Array.Empty<IUnturnedUIData>();
    }
    public IEnumerable<IUnturnedUIData> EnumerateData(UnturnedUI owner)
    {
        ThreadUtil.assertIsGameThread();

        return _ownerData.TryGetValue(owner, out List<IUnturnedUIData> data) ? data : Array.Empty<IUnturnedUIData>();
    }

    public IEnumerable<IUnturnedUIData> EnumerateData(UnturnedUIElement element)
    {
        ThreadUtil.assertIsGameThread();

        return _elementData.TryGetValue(element, out List<IUnturnedUIData> data) ? data : Array.Empty<IUnturnedUIData>();
    }
    public void Dispose()
    {
        if (_disposed)
            return;
        foreach (List<IUnturnedUIData> data in _playerData.Values)
        {
            foreach (IDisposable disp in data.OfType<IDisposable>())
                disp.Dispose();
            data.Clear();
        }
        _playerData.Clear();
        _playerData = null!;

        foreach (List<IUnturnedUIData> data in _elementData.Values)
        {
            foreach (IDisposable disp in data.OfType<IDisposable>())
                disp.Dispose();
            data.Clear();
        }
        _elementData.Clear();
        _elementData = null!;

        foreach (List<IUnturnedUIData> data in _ownerData.Values)
        {
            foreach (IDisposable disp in data.OfType<IDisposable>())
                disp.Dispose();
            data.Clear();
        }
        _ownerData.Clear();
        _ownerData = null!;

        _disposed = true;
    }
}