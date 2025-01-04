using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Uncreated.Framework.UI.Data;

namespace Uncreated.Framework.UI;

/// <summary>
/// Manages instances of <see cref="IUnturnedUIData"/>. Available through <see cref="UnturnedUIDataSource"/>.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Advanced)]
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
            if (!UnturnedUIProvider.Instance.IsValidThread() && old.RequiresMainThread)
            {
                UnturnedUIProvider.Instance.DispatchToValidThread(() =>
                {
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
        if (UnturnedUIProvider.Instance.IsValidThread())
        {
            _isListeningToPlayerDisconnect = true;
            UnturnedUIProvider.Instance.OnDisconnect += OnPlayerDisconnected;
        }
        else
        {
            UnturnedUIProvider.Instance.DispatchToValidThread(() =>
            {
                if (_isListeningToPlayerDisconnect)
                    return;

                _isListeningToPlayerDisconnect = true;
                UnturnedUIProvider.Instance.OnDisconnect += OnPlayerDisconnected;
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
        if (_instance.RequiresMainThread && !UnturnedUIProvider.Instance.IsValidThread())
        {
            IUnturnedUIData data2 = data;
            UnturnedUIProvider.Instance.DispatchToValidThread(() =>
            {
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
        if (_instance.RequiresMainThread && !UnturnedUIProvider.Instance.IsValidThread())
        {
            UnturnedUIElement element2 = element;
            UnturnedUIProvider.Instance.DispatchToValidThread(() =>
            {
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
        if (_instance.RequiresMainThread && !UnturnedUIProvider.Instance.IsValidThread())
        {
            UnturnedUI owner2 = owner;
            UnturnedUIProvider.Instance.DispatchToValidThread(() =>
            {
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
        if (_instance.RequiresMainThread && !UnturnedUIProvider.Instance.IsValidThread())
        {
            CSteamID player2 = player;
            UnturnedUIProvider.Instance.DispatchToValidThread(() =>
            {
                _instance.RemovePlayer(player2);
            });
        }
        else _instance.RemovePlayer(player);
    }

    /// <summary>
    /// Get the data, if it exists, for a given <paramref name="player"/> and <paramref name="element"/>, otheewise create and add it
    /// </summary>
    /// <exception cref="NotSupportedException">Not ran on main thread if <see cref="IUnturnedUIDataSource.RequiresMainThread"/> is <see langword="true"/>.</exception>
    public static TData GetOrAddData<TData>(CSteamID player, UnturnedUIElement element, Func<CSteamID, UnturnedUIElement, TData> func) where TData : class, IUnturnedUIData
    {
        if (_instance.RequiresMainThread)
            UnturnedUIProvider.Instance.AssertValidThread();

        TData? data = _instance.GetData<TData>(player, element);
        if (data == null)
        {
            data = func(player, element);
            if (data.Player.m_SteamID != player.m_SteamID || data.Element != element || data.Owner != element.Owner)
                throw new InvalidOperationException("Created data does not store the same player and element as expected.");
            _instance.AddData(data);
        }

        return data;
    }

    /// <summary>
    /// Get the data, if it exists, for a given <paramref name="player"/> and <paramref name="ui"/>, otheewise create and add it
    /// </summary>
    /// <exception cref="NotSupportedException">Not ran on main thread if <see cref="IUnturnedUIDataSource.RequiresMainThread"/> is <see langword="true"/>.</exception>
    public static TData GetOrAddData<TData>(CSteamID player, UnturnedUI ui, Func<CSteamID, UnturnedUI, TData> func) where TData : class, IUnturnedUIData
    {
        if (_instance.RequiresMainThread)
            UnturnedUIProvider.Instance.AssertValidThread();

        TData? data = _instance.GetData<TData>(player, ui);
        if (data == null)
        {
            data = func(player, ui);
            if (data.Player.m_SteamID != player.m_SteamID || data.Element != null || data.Owner != ui)
                throw new InvalidOperationException("Created data does not store the same player and UI as expected.");
            _instance.AddData(data);
        }

        return data;
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
        UnturnedUIProvider.Instance.AssertValidThread();

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
        UnturnedUIProvider.Instance.AssertValidThread();
        
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
        UnturnedUIProvider.Instance.AssertValidThread();
        
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
        UnturnedUIProvider.Instance.AssertValidThread();
        
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
        UnturnedUIProvider.Instance.AssertValidThread();

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
        UnturnedUIProvider.Instance.AssertValidThread();

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
        UnturnedUIProvider.Instance.AssertValidThread();

        return _playerData.TryGetValue(player.m_SteamID, out List<IUnturnedUIData> data) ? data : Array.Empty<IUnturnedUIData>();
    }

    /// <inheritdoc />
    IEnumerable<IUnturnedUIData> IUnturnedUIDataSource.EnumerateData(UnturnedUI owner)
    {
        UnturnedUIProvider.Instance.AssertValidThread();

        return _ownerData.TryGetValue(owner, out List<IUnturnedUIData> data) ? data : Array.Empty<IUnturnedUIData>();
    }

    /// <inheritdoc />
    IEnumerable<IUnturnedUIData> IUnturnedUIDataSource.EnumerateData(UnturnedUIElement element)
    {
        UnturnedUIProvider.Instance.AssertValidThread();

        return _elementData.TryGetValue(element, out List<IUnturnedUIData> data) ? data : Array.Empty<IUnturnedUIData>();
    }

    /// <inheritdoc />
    void IDisposable.Dispose()
    {
        UnturnedUIProvider.Instance.AssertValidThread();

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