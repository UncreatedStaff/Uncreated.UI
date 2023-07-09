using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Uncreated.Framework.UI.Data;
using UnityEngine;

namespace Uncreated.Framework.UI;

public interface IUnturnedUIDataSource
{
    bool RequiresMainThread { get; }
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
    bool IUnturnedUIDataSource.RequiresMainThread => true;
    /// <remarks>Thread-safe.</remarks>
    public static void AddData(IUnturnedUIData data)
    {
        if (_instance.RequiresMainThread && ThreadQueue.Queue is { IsMainThread: false })
            ThreadQueue.Queue.RunOnMainThread(() => _instance.AddData(data));
        else _instance.AddData(data);
    }
    /// <remarks>Thread-safe.</remarks>
    public static void RemoveElement(UnturnedUIElement element)
    {
        if (_instance.RequiresMainThread && ThreadQueue.Queue is { IsMainThread: false })
            ThreadQueue.Queue.RunOnMainThread(() => _instance.RemoveElement(element));
        else _instance.RemoveElement(element);
    }
    /// <remarks>Thread-safe.</remarks>
    public static void RemoveOwner(UnturnedUI owner)
    {
        if (_instance.RequiresMainThread && ThreadQueue.Queue is { IsMainThread: false })
            ThreadQueue.Queue.RunOnMainThread(() => _instance.RemoveOwner(owner));
        else _instance.RemoveOwner(owner);
    }
    /// <remarks>Thread-safe.</remarks>
    public static void RemovePlayer(CSteamID player)
    {
        if (_instance.RequiresMainThread && ThreadQueue.Queue is { IsMainThread: false })
            ThreadQueue.Queue.RunOnMainThread(() => _instance.RemovePlayer(player));
        else _instance.RemovePlayer(player);
    }
    /// <exception cref="NotSupportedException">Not ran on main thread if required.</exception>
    public static TData? GetData<TData>(CSteamID player, UnturnedUIElement element) where TData : class, IUnturnedUIData
        => _instance.GetData<TData>(player, element);

    /// <exception cref="NotSupportedException">Not ran on main thread if required.</exception>
    public static IEnumerable<IUnturnedUIData> EnumerateData(CSteamID player)
        => _instance.EnumerateData(player);

    /// <exception cref="NotSupportedException">Not ran on main thread if required.</exception>
    public static IEnumerable<IUnturnedUIData> EnumerateData(UnturnedUI owner)
        => _instance.EnumerateData(owner);

    /// <exception cref="NotSupportedException">Not ran on main thread if required.</exception>
    public static IEnumerable<IUnturnedUIData> EnumerateData(UnturnedUIElement element)
        => _instance.EnumerateData(element);

    void IUnturnedUIDataSource.AddData(IUnturnedUIData data)
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
    void IUnturnedUIDataSource.RemoveElement(UnturnedUIElement element)
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
    void IUnturnedUIDataSource.RemoveOwner(UnturnedUI owner)
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
    void IUnturnedUIDataSource.RemovePlayer(CSteamID player)
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
    TData? IUnturnedUIDataSource.GetData<TData>(CSteamID player, UnturnedUIElement element) where TData : class
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
    IEnumerable<IUnturnedUIData> IUnturnedUIDataSource.EnumerateData(CSteamID player)
    {
        ThreadUtil.assertIsGameThread();

        return _playerData.TryGetValue(player.m_SteamID, out List<IUnturnedUIData> data) ? data : Array.Empty<IUnturnedUIData>();
    }
    IEnumerable<IUnturnedUIData> IUnturnedUIDataSource.EnumerateData(UnturnedUI owner)
    {
        ThreadUtil.assertIsGameThread();

        return _ownerData.TryGetValue(owner, out List<IUnturnedUIData> data) ? data : Array.Empty<IUnturnedUIData>();
    }

    IEnumerable<IUnturnedUIData> IUnturnedUIDataSource.EnumerateData(UnturnedUIElement element)
    {
        ThreadUtil.assertIsGameThread();

        return _elementData.TryGetValue(element, out List<IUnturnedUIData> data) ? data : Array.Empty<IUnturnedUIData>();
    }
    void IDisposable.Dispose()
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