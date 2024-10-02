using Cysharp.Threading.Tasks;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Uncreated.Framework.UI.Data;

namespace Uncreated.Framework.UI;

/// <summary>
/// Manages instances of <see cref="IUnturnedUIData"/>. Available through <see cref="UnturnedUIDataSource"/>.
/// </summary>
public interface IUnturnedUIDataSource : IDisposable
{
    /// <summary>
    /// If all functions must be ran on the main thread.
    /// </summary>
    bool RequiresMainThread { get; }

    /// <summary>
    /// Enumerate through all data linked to a <paramref name="player"/>.
    /// </summary>
    /// <exception cref="NotSupportedException">Not ran on main thread if <see cref="RequiresMainThread"/> is <see langword="true"/>.</exception>
    IEnumerable<IUnturnedUIData> EnumerateData(CSteamID player);

    /// <summary>
    /// Enumerate through all data linked to a UI <paramref name="owner"/>.
    /// </summary>
    /// <exception cref="NotSupportedException">Not ran on main thread if <see cref="RequiresMainThread"/> is <see langword="true"/>.</exception>
    IEnumerable<IUnturnedUIData> EnumerateData(UnturnedUI owner);

    /// <summary>
    /// Enumerate through all data linked to a UI <paramref name="element"/>.
    /// </summary>
    /// <exception cref="NotSupportedException">Not ran on main thread if <see cref="RequiresMainThread"/> is <see langword="true"/>.</exception>
    IEnumerable<IUnturnedUIData> EnumerateData(UnturnedUIElement element);

    /// <summary>
    /// Get the data, if it exists, for a given <paramref name="player"/> and <paramref name="element"/>.
    /// </summary>
    /// <exception cref="NotSupportedException">Not ran on main thread if <see cref="RequiresMainThread"/> is <see langword="true"/>.</exception>
    TData? GetData<TData>(CSteamID player, UnturnedUIElement element) where TData : class, IUnturnedUIData;
    
    /// <summary>
    /// Get the data, if it exists, for a given <paramref name="player"/> and <paramref name="ui"/> where the element is <see langword="null"/>.
    /// </summary>
    /// <exception cref="NotSupportedException">Not ran on main thread if <see cref="RequiresMainThread"/> is <see langword="true"/>.</exception>
    TData? GetData<TData>(CSteamID player, UnturnedUI ui) where TData : class, IUnturnedUIData;

    /// <summary>
    /// Add new UI data.
    /// </summary>
    /// <exception cref="NotSupportedException">Not ran on main thread if <see cref="RequiresMainThread"/> is <see langword="true"/>.</exception>
    void AddData(IUnturnedUIData data);

    /// <summary>
    /// Remove all data for a given <paramref name="player"/>.
    /// </summary>
    /// <exception cref="NotSupportedException">Not ran on main thread if <see cref="RequiresMainThread"/> is <see langword="true"/>.</exception>
    void RemovePlayer(CSteamID player);

    /// <summary>
    /// Remove all data for a given UI <paramref name="element"/>.
    /// </summary>
    /// <exception cref="NotSupportedException">Not ran on main thread if <see cref="RequiresMainThread"/> is <see langword="true"/>.</exception>
    void RemoveElement(UnturnedUIElement element);

    /// <summary>
    /// Remove all data for a given UI <paramref name="owner"/>.
    /// </summary>
    /// <exception cref="NotSupportedException">Not ran on main thread if <see cref="RequiresMainThread"/> is <see langword="true"/>.</exception>
    void RemoveOwner(UnturnedUI owner);
}

/// <summary>
/// Manages instances of <see cref="IUnturnedUIData"/>.
/// </summary>
public class UnturnedUIDataSource : IUnturnedUIDataSource
{
    private static IUnturnedUIDataSource _instance = new UnturnedUIDataSource();

    /// <summary>
    /// Singleton instance of <see cref="IUnturnedUIDataSource"/>.
    /// </summary>
    /// <remarks>The old value will be disposed.</remarks>
    public static IUnturnedUIDataSource Instance
    {
        get => _instance;
        set
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            IUnturnedUIDataSource old = Interlocked.Exchange(ref _instance, value);
            if (!Thread.CurrentThread.IsGameThread() && old.RequiresMainThread)
            {
                UniTask.Create(async () =>
                {
                    await UniTask.SwitchToMainThread();
                    try
                    {
                        old.Dispose();
                    }
                    catch
                    {
                        // ignored
                    }
                });
            }
            else
            {
                try
                {
                    old.Dispose();
                }
                catch
                {
                    // ignored
                }
            }
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized, false);
        }
    }

    private bool _disposed;
    private bool _isListeningToPlayerDisconnect;
    private Dictionary<ulong, List<IUnturnedUIData>> _playerData = new Dictionary<ulong, List<IUnturnedUIData>>(96);
    private Dictionary<UnturnedUI, List<IUnturnedUIData>> _ownerData = new Dictionary<UnturnedUI, List<IUnturnedUIData>>(256);
    private Dictionary<UnturnedUIElement, List<IUnturnedUIData>> _elementData = new Dictionary<UnturnedUIElement, List<IUnturnedUIData>>(2048);

    /// <inheritdoc />
    bool IUnturnedUIDataSource.RequiresMainThread => true;

    public UnturnedUIDataSource()
    {
        if (Thread.CurrentThread.IsGameThread())
        {
            _isListeningToPlayerDisconnect = true;
            Provider.onServerDisconnected += OnPlayerDisconnected;
        }
        else
        {
            UniTask.Create(async () =>
            {
                await UniTask.SwitchToMainThread();
                if (_isListeningToPlayerDisconnect)
                    return;
                
                _isListeningToPlayerDisconnect = true;
                Provider.onServerDisconnected += OnPlayerDisconnected;
            });
        }
    }

    // Provider.onServerDisconnected
    private void OnPlayerDisconnected(CSteamID playerId)
    {
        ((IUnturnedUIDataSource)this).RemovePlayer(playerId);
    }

    /// <summary>
    /// Add new UI data.
    /// </summary>
    /// <remarks>Thread-safe.</remarks>
    public static void AddData(IUnturnedUIData data)
    {
        if (_instance.RequiresMainThread && !Thread.CurrentThread.IsGameThread())
        {
            IUnturnedUIData data2 = data;
            UniTask.Create(async () =>
            {
                await UniTask.SwitchToMainThread();
                _instance.AddData(data2);
            });
        }
        else _instance.AddData(data);
    }

    /// <summary>
    /// Remove all data for a given UI <paramref name="element"/>.
    /// </summary>
    /// <remarks>Thread-safe.</remarks>
    public static void RemoveElement(UnturnedUIElement element)
    {
        if (_instance.RequiresMainThread && !Thread.CurrentThread.IsGameThread())
        {
            UnturnedUIElement element2 = element;
            UniTask.Create(async () =>
            {
                await UniTask.SwitchToMainThread();
                _instance.RemoveElement(element2);
            });
        }
        else _instance.RemoveElement(element);
    }

    /// <summary>
    /// Remove all data for a given UI <paramref name="owner"/>.
    /// </summary>
    /// <remarks>Thread-safe.</remarks>
    public static void RemoveOwner(UnturnedUI owner)
    {
        if (_instance.RequiresMainThread && !Thread.CurrentThread.IsGameThread())
        {
            UnturnedUI owner2 = owner;
            UniTask.Create(async () =>
            {
                await UniTask.SwitchToMainThread();
                _instance.RemoveOwner(owner2);
            });
        }
        else _instance.RemoveOwner(owner);
    }

    /// <summary>
    /// Remove all data for a given <paramref name="player"/>.
    /// </summary>
    /// <remarks>Thread-safe.</remarks>
    public static void RemovePlayer(CSteamID player)
    {
        if (_instance.RequiresMainThread && !Thread.CurrentThread.IsGameThread())
        {
            CSteamID player2 = player;
            UniTask.Create(async () =>
            {
                await UniTask.SwitchToMainThread();
                _instance.RemovePlayer(player2);
            });
        }
        else _instance.RemovePlayer(player);
    }

    /// <summary>
    /// Get the data, if it exists, for a given <paramref name="player"/> and <paramref name="element"/>.
    /// </summary>
    /// <exception cref="NotSupportedException">Not ran on main thread if <see cref="IUnturnedUIDataSource.RequiresMainThread"/> is <see langword="true"/>.</exception>
    public static TData? GetData<TData>(CSteamID player, UnturnedUIElement element) where TData : class, IUnturnedUIData
        => _instance.GetData<TData>(player, element);

    /// <summary>
    /// Get the data, if it exists, for a given <paramref name="player"/> and <paramref name="ui"/> where the element is <see langword="null"/>.
    /// </summary>
    /// <exception cref="NotSupportedException">Not ran on main thread if <see cref="IUnturnedUIDataSource.RequiresMainThread"/> is <see langword="true"/>.</exception>
    public static TData? GetData<TData>(CSteamID player, UnturnedUI ui) where TData : class, IUnturnedUIData
        => _instance.GetData<TData>(player, ui);

    /// <summary>
    /// Remove all data for a given <paramref name="player"/>.
    /// </summary>
    /// <exception cref="NotSupportedException">Not ran on main thread if <see cref="IUnturnedUIDataSource.RequiresMainThread"/> is <see langword="true"/>.</exception>
    public static IEnumerable<IUnturnedUIData> EnumerateData(CSteamID player)
        => _instance.EnumerateData(player);

    /// <summary>
    /// Enumerate through all data linked to a UI <paramref name="owner"/>.
    /// </summary>
    /// <exception cref="NotSupportedException">Not ran on main thread if <see cref="IUnturnedUIDataSource.RequiresMainThread"/> is <see langword="true"/>.</exception>
    public static IEnumerable<IUnturnedUIData> EnumerateData(UnturnedUI owner)
        => _instance.EnumerateData(owner);

    /// <summary>
    /// Enumerate through all data linked to a UI <paramref name="element"/>.
    /// </summary>
    /// <exception cref="NotSupportedException">Not ran on main thread if <see cref="IUnturnedUIDataSource.RequiresMainThread"/> is <see langword="true"/>.</exception>
    public static IEnumerable<IUnturnedUIData> EnumerateData(UnturnedUIElement element)
        => _instance.EnumerateData(element);

    /// <inheritdoc />
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

    /// <inheritdoc />
    void IUnturnedUIDataSource.RemoveElement(UnturnedUIElement element)
    {
        ThreadUtil.assertIsGameThread();
        
        if (!_elementData.Remove(element, out List<IUnturnedUIData> list))
            return;
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

    /// <inheritdoc />
    void IUnturnedUIDataSource.RemoveOwner(UnturnedUI owner)
    {
        ThreadUtil.assertIsGameThread();
        
        if (!_ownerData.Remove(owner, out List<IUnturnedUIData> list))
            return;
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

    /// <inheritdoc />
    void IUnturnedUIDataSource.RemovePlayer(CSteamID player)
    {
        ThreadUtil.assertIsGameThread();
        
        if (!_playerData.Remove(player.m_SteamID, out List<IUnturnedUIData> list))
            return;
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

    /// <inheritdoc />
    TData? IUnturnedUIDataSource.GetData<TData>(CSteamID player, UnturnedUIElement element) where TData : class
    {
        ThreadUtil.assertIsGameThread();

        if (!_elementData.TryGetValue(element, out List<IUnturnedUIData> data))
            return null;

        for (int i = data.Count - 1; i >= 0; --i)
        {
            if (data[i] is TData data2 && data2.Player.m_SteamID == player.m_SteamID)
                return data2;
        }

        return null;
    }

    /// <inheritdoc />
    TData? IUnturnedUIDataSource.GetData<TData>(CSteamID player, UnturnedUI ui) where TData : class
    {
        ThreadUtil.assertIsGameThread();

        if (!_ownerData.TryGetValue(ui, out List<IUnturnedUIData> data))
            return null;

        for (int i = data.Count - 1; i >= 0; --i)
        {
            if (data[i] is TData data2 && data2.Player.m_SteamID == player.m_SteamID && data2.Element == null)
                return data2;
        }

        return null;
    }

    /// <inheritdoc />
    IEnumerable<IUnturnedUIData> IUnturnedUIDataSource.EnumerateData(CSteamID player)
    {
        ThreadUtil.assertIsGameThread();

        return _playerData.TryGetValue(player.m_SteamID, out List<IUnturnedUIData> data) ? data : Array.Empty<IUnturnedUIData>();
    }

    /// <inheritdoc />
    IEnumerable<IUnturnedUIData> IUnturnedUIDataSource.EnumerateData(UnturnedUI owner)
    {
        ThreadUtil.assertIsGameThread();

        return _ownerData.TryGetValue(owner, out List<IUnturnedUIData> data) ? data : Array.Empty<IUnturnedUIData>();
    }

    /// <inheritdoc />
    IEnumerable<IUnturnedUIData> IUnturnedUIDataSource.EnumerateData(UnturnedUIElement element)
    {
        ThreadUtil.assertIsGameThread();

        return _elementData.TryGetValue(element, out List<IUnturnedUIData> data) ? data : Array.Empty<IUnturnedUIData>();
    }

    /// <inheritdoc />
    void IDisposable.Dispose()
    {
        ThreadUtil.assertIsGameThread();

        if (_isListeningToPlayerDisconnect)
        {
            _isListeningToPlayerDisconnect = false;
            Provider.onServerDisconnected += OnPlayerDisconnected;
        }

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