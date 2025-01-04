using Microsoft.Extensions.Logging;
using SDG.Framework.Utilities;
using SDG.NetTransport;
using SDG.Unturned;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Threading;
using Action = System.Action;

namespace Uncreated.Framework.UI;

/// <summary>
/// Thread-safe abstraction surrounding the vanilla UI API.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Advanced)]
public class UnturnedUIProvider : IUnturnedUIProvider, IDisposable
{
    [ThreadStatic]
    private static bool _isGameThread;

    private static ILogger? _logger;

    private static bool _isGameThreadSetup;

    // ReSharper disable once ReplaceWithFieldKeyword
    private static IUnturnedUIProvider _instance = new UnturnedUIProvider();

    private readonly ConcurrentQueue<Action> _actions = new ConcurrentQueue<Action>();

    /// <summary>
    /// Adds a layer of abstraction between Uncreated.UI and <see cref="EffectManager"/>.
    /// </summary>
    public static IUnturnedUIProvider Instance
    {
        get => _instance;
        set
        {
            IUnturnedUIProvider? oldValue = Interlocked.Exchange(ref _instance, value);
            if (oldValue is not IDisposable disp)
                return;

            try
            {
                disp.Dispose();
            }
            catch (Exception ex)
            {
                GetLogger().LogError(ex, "Failed to dispose UI provider: {0}.", oldValue.GetType());
            }
        }
    }

    public void DispatchToValidThread(Action action)
    {
        if (IsGameThread())
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                GetLogger().LogError(ex, "Error invoking main thread dispatch (without dispatching).");
            }
        }
        else
        {
            _actions.Enqueue(action);
        }
    }

    public virtual event EffectManager.EffectButtonClickedHandler OnButtonClicked
    {
        // field events technically aren't thread-safe
        add
        {
            if (IsGameThread())
            {
                EffectManager.onEffectButtonClicked += value;
            }
            else
            {
                AddButtonPressedListenerState s = new AddButtonPressedListenerState(value);
                _actions.Enqueue(s.Add);
            }
        }
        remove
        {
            if (IsGameThread())
            {
                EffectManager.onEffectButtonClicked -= value;
            }
            else
            {
                AddButtonPressedListenerState s = new AddButtonPressedListenerState(value);
                _actions.Enqueue(s.Remove);
            }
        }
    }

    public virtual event EffectManager.EffectTextCommittedHandler OnTextCommitted
    {
        add
        {
            if (IsGameThread())
            {
                EffectManager.onEffectTextCommitted += value;
            }
            else
            {
                AddTextCommittedListenerState s = new AddTextCommittedListenerState(value);
                _actions.Enqueue(s.Add);
            }
        }
        remove
        {
            if (IsGameThread())
            {
                EffectManager.onEffectTextCommitted -= value;
            }
            else
            {
                AddTextCommittedListenerState s = new AddTextCommittedListenerState(value);
                _actions.Enqueue(s.Remove);
            }
        }
    }

    public virtual event Provider.ServerDisconnected OnDisconnect
    {
        add
        {
            if (IsGameThread())
            {
                Provider.onServerDisconnected += value;
            }
            else
            {
                AddProviderDisconnectedListenerState s = new AddProviderDisconnectedListenerState(value);
                _actions.Enqueue(s.Add);
            }
        }
        remove
        {
            if (IsGameThread())
            {
                Provider.onServerDisconnected -= value;
            }
            else
            {
                AddProviderDisconnectedListenerState s = new AddProviderDisconnectedListenerState(value);
                _actions.Enqueue(s.Remove);
            }
        }
    }

    public bool AreAssetsStillLoading => Assets.isLoading;

    static UnturnedUIProvider() { }

    private UnturnedUIProvider()
    {
        TimeUtility.updated += OnUpdate;
        if (!Thread.CurrentThread.IsGameThread())
            return;

        _isGameThread = true;
        _isGameThreadSetup = true;
    }

    void IDisposable.Dispose()
    {
        TimeUtility.updated -= OnUpdate;
    }

    private static ILogger GetLogger()
    {
        return _logger ??= GlobalLogger.Instance.CreateLogger(typeof(UnturnedUIProvider));
    }

    /// <summary>
    /// Set <see cref="Instance"/> back to the vanilla <see cref="IUnturnedUIProvider"/> if it isn't already.
    /// </summary>
    public static void Reset()
    {
        if (_instance.GetType() != typeof(UnturnedUIProvider))
            Instance = new UnturnedUIProvider();
    }

    private static bool IsGameThread()
    {
        return _isGameThreadSetup ? _isGameThread : TrySetupGameThread();
    }

    private static bool TrySetupGameThread()
    {
        if (!Thread.CurrentThread.IsGameThread())
            return false;

        _isGameThread = true;
        _isGameThreadSetup = true;
        return true;
    }

    public virtual bool IsValidThread()
    {
        return IsGameThread();
    }

    private void OnUpdate()
    {
        while (_actions.TryDequeue(out Action action))
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                GetLogger().LogError(ex, "Error invoking main thread dispatch.");
            }
        }
    }

    public virtual void ClearById(ushort id, ITransportConnection connection)
    {
        if (IsGameThread())
        {
            EffectManager.askEffectClearByID(id, connection);
        }
        else
        {
            ClearByIdState s = new ClearByIdState(id, connection);
            _actions.Enqueue(s.Invoke);
        }
    }

    public virtual void ClearByIdGlobal(ushort id)
    {
        if (IsGameThread())
        {
            EffectManager.ClearEffectByID_AllPlayers(id);
        }
        else
        {
            ClearByIdState s = new ClearByIdState(id, null);
            _actions.Enqueue(s.Invoke);
        }
    }

    public virtual void SetElementVisibility(short key, ITransportConnection connection, bool isReliable, string path, bool state)
    {
        if (IsGameThread())
        {
            EffectManager.sendUIEffectVisibility(key, connection, isReliable, path, state);
        }
        else
        {
            SetElementVisibilityState s = new SetElementVisibilityState(key, connection, isReliable, path, state);
            _actions.Enqueue(s.Invoke);
        }
    }

    public virtual void SetElementText(short key, ITransportConnection connection, bool isReliable, string path, string text)
    {
        if (IsGameThread())
        {
            EffectManager.sendUIEffectText(key, connection, isReliable, path, text);
        }
        else
        {
            SetElementTextState s = new SetElementTextState(key, connection, isReliable, path, text);
            _actions.Enqueue(s.Invoke);
        }
    }

    public virtual void SetElementImageUrl(short key, ITransportConnection connection, bool isReliable, string path, string imageUrl, bool shouldCache, bool forceRefresh)
    {
        if (IsGameThread())
        {
            EffectManager.sendUIEffectImageURL(key, connection, isReliable, path, imageUrl, shouldCache, forceRefresh);
        }
        else
        {
            SetElementImageUrlState s = new SetElementImageUrlState(key, connection, isReliable, path, imageUrl, shouldCache, forceRefresh);
            _actions.Enqueue(s.Invoke);
        }
    }

    public virtual void SendUI(ushort id, short key, ITransportConnection connection, bool isReliable)
    {
        if (IsGameThread())
        {
            EffectManager.sendUIEffect(id, key, connection, isReliable);
        }
        else
        {
            SendUIState s = new SendUIState(id, key, connection, isReliable);
            _actions.Enqueue(s.Invoke);
        }
    }

    public virtual void SendUI(ushort id, short key, ITransportConnection connection, bool isReliable, string arg0)
    {
        if (IsGameThread())
        {
            EffectManager.sendUIEffect(id, key, connection, isReliable, arg0);
        }
        else
        {
            SendUIState s = new SendUIState(id, key, connection, isReliable, arg0);
            _actions.Enqueue(s.Invoke);
        }
    }

    public virtual void SendUI(ushort id, short key, ITransportConnection connection, bool isReliable, string arg0, string arg1)
    {
        if (IsGameThread())
        {
            EffectManager.sendUIEffect(id, key, connection, isReliable, arg0, arg1);
        }
        else
        {
            SendUIState s = new SendUIState(id, key, connection, isReliable, arg0, arg1);
            _actions.Enqueue(s.Invoke);
        }
    }

    public virtual void SendUI(ushort id, short key, ITransportConnection connection, bool isReliable, string arg0, string arg1, string arg2)
    {
        if (IsGameThread())
        {
            EffectManager.sendUIEffect(id, key, connection, isReliable, arg0, arg1, arg2);
        }
        else
        {
            SendUIState s = new SendUIState(id, key, connection, isReliable, arg0, arg1, arg2);
            _actions.Enqueue(s.Invoke);
        }
    }

    public virtual void SendUI(ushort id, short key, ITransportConnection connection, bool isReliable, string arg0, string arg1, string arg2, string arg3)
    {
        if (IsGameThread())
        {
            EffectManager.sendUIEffect(id, key, connection, isReliable, arg0, arg1, arg2, arg3);
        }
        else
        {
            SendUIState s = new SendUIState(id, key, connection, isReliable, arg0, arg1, arg2, arg3);
            _actions.Enqueue(s.Invoke);
        }
    }

    public virtual void SendUIGlobal(ushort id, short key, bool isReliable)
    {
        if (IsGameThread())
        {
            EffectManager.sendUIEffect(id, key, isReliable);
        }
        else
        {
            SendUIState s = new SendUIState(id, key, null, isReliable);
            _actions.Enqueue(s.Invoke);
        }
    }

    public virtual void SendUIGlobal(ushort id, short key, bool isReliable, string arg0)
    {
        if (IsGameThread())
        {
            EffectManager.sendUIEffect(id, key, isReliable, arg0);
        }
        else
        {
            SendUIState s = new SendUIState(id, key, null, isReliable, arg0);
            _actions.Enqueue(s.Invoke);
        }
    }

    public virtual void SendUIGlobal(ushort id, short key, bool isReliable, string arg0, string arg1)
    {
        if (IsGameThread())
        {
            EffectManager.sendUIEffect(id, key, isReliable, arg0, arg1);
        }
        else
        {
            SendUIState s = new SendUIState(id, key, null, isReliable, arg0, arg1);
            _actions.Enqueue(s.Invoke);
        }
    }

    public virtual void SendUIGlobal(ushort id, short key, bool isReliable, string arg0, string arg1, string arg2)
    {
        if (IsGameThread())
        {
            EffectManager.sendUIEffect(id, key, isReliable, arg0, arg1, arg2);
        }
        else
        {
            SendUIState s = new SendUIState(id, key, null, isReliable, arg0, arg1, arg2);
            _actions.Enqueue(s.Invoke);
        }
    }

    public virtual void SendUIGlobal(ushort id, short key, bool isReliable, string arg0, string arg1, string arg2, string arg3)
    {
        if (IsGameThread())
        {
            EffectManager.sendUIEffect(id, key, isReliable, arg0, arg1, arg2, arg3);
        }
        else
        {
            SendUIState s = new SendUIState(id, key, null, isReliable, arg0, arg1, arg2, arg3);
            _actions.Enqueue(s.Invoke);
        }
    }

    public virtual EffectAsset? GetEffectAsset(ushort id) => Assets.find(EAssetType.EFFECT, id) as EffectAsset;

    public virtual EffectAsset? GetEffectAsset(Guid guid) => Assets.find<EffectAsset>(guid);

    private class ClearByIdState
    {
        private readonly ushort _id;
        private readonly ITransportConnection? _connection;

        public ClearByIdState(ushort id, ITransportConnection? connection)
        {
            _id = id;
            _connection = connection;
        }

        public void Invoke()
        {
            if (_connection is null)
                EffectManager.ClearEffectByID_AllPlayers(_id);
            else
                EffectManager.askEffectClearByID(_id, _connection);
        }
    }

    private class SetElementVisibilityState
    {
        private readonly short _key;
        private readonly ITransportConnection _connection;
        private readonly bool _isReliable;
        private readonly string _path;
        private readonly bool _state;

        public SetElementVisibilityState(short key, ITransportConnection connection, bool isReliable, string path, bool state)
        {
            _key = key;
            _connection = connection;
            _isReliable = isReliable;
            _path = path;
            _state = state;
        }

        public void Invoke()
        {
            EffectManager.sendUIEffectVisibility(_key, _connection, _isReliable, _path, _state);
        }
    }

    private class SetElementTextState
    {
        private readonly short _key;
        private readonly ITransportConnection _connection;
        private readonly bool _isReliable;
        private readonly string _path;
        private readonly string _text;

        public SetElementTextState(short key, ITransportConnection connection, bool isReliable, string path, string text)
        {
            _key = key;
            _connection = connection;
            _isReliable = isReliable;
            _path = path;
            _text = text;
        }

        public void Invoke()
        {
            EffectManager.sendUIEffectText(_key, _connection, _isReliable, _path, _text);
        }
    }

    private class SetElementImageUrlState
    {
        private readonly short _key;
        private readonly ITransportConnection _connection;
        private readonly bool _isReliable;
        private readonly string _path;
        private readonly string _imageUrl;
        private readonly bool _shouldCache;
        private readonly bool _forceRefresh;

        public SetElementImageUrlState(short key, ITransportConnection connection, bool isReliable, string path, string imageUrl, bool shouldCache, bool forceRefresh)
        {
            _key = key;
            _connection = connection;
            _isReliable = isReliable;
            _path = path;
            _imageUrl = imageUrl;
            _shouldCache = shouldCache;
            _forceRefresh = forceRefresh;
        }

        public void Invoke()
        {
            EffectManager.sendUIEffectImageURL(_key, _connection, _isReliable, _path, _imageUrl, _shouldCache, _forceRefresh);
        }
    }

    public class SendUIState
    {
        private readonly int _argCt;
        private readonly string? _arg0;
        private readonly string? _arg1;
        private readonly string? _arg2;
        private readonly string? _arg3;
        private readonly ushort _id;
        private readonly short _key;
        private readonly ITransportConnection? _connection;
        private readonly bool _isReliable;

        public SendUIState(ushort id, short key, ITransportConnection? connection, bool isReliable)
        {
            _argCt = 0;
            _id = id;
            _key = key;
            _connection = connection;
            _isReliable = isReliable;
        }
        public SendUIState(ushort id, short key, ITransportConnection? connection, bool isReliable, string? arg0)
        {
            _argCt = 1;
            _arg0 = arg0;
            _id = id;
            _key = key;
            _connection = connection;
            _isReliable = isReliable;
        }
        public SendUIState(ushort id, short key, ITransportConnection? connection, bool isReliable, string? arg0, string? arg1)
        {
            _argCt = 2;
            _arg0 = arg0;
            _arg1 = arg1;
            _id = id;
            _key = key;
            _connection = connection;
            _isReliable = isReliable;
        }
        public SendUIState(ushort id, short key, ITransportConnection? connection, bool isReliable, string? arg0, string? arg1, string? arg2)
        {
            _argCt = 3;
            _arg0 = arg0;
            _arg1 = arg1;
            _arg2 = arg2;
            _id = id;
            _key = key;
            _connection = connection;
            _isReliable = isReliable;
        }
        public SendUIState(ushort id, short key, ITransportConnection? connection, bool isReliable, string? arg0, string? arg1, string? arg2, string? arg3)
        {
            _argCt = 4;
            _arg0 = arg0;
            _arg1 = arg1;
            _arg2 = arg2;
            _arg3 = arg3;
            _id = id;
            _key = key;
            _connection = connection;
            _isReliable = isReliable;
        }

        public void Invoke()
        {
            if (_connection == null)
            {
                switch (_argCt)
                {
                    case 0:
                        EffectManager.sendUIEffect(_id, _key, _isReliable);
                        break;
                    case 1:
                        EffectManager.sendUIEffect(_id, _key, _isReliable, _arg0);
                        break;
                    case 2:
                        EffectManager.sendUIEffect(_id, _key, _isReliable, _arg0, _arg1);
                        break;
                    case 3:
                        EffectManager.sendUIEffect(_id, _key, _isReliable, _arg0, _arg1, _arg2);
                        break;
                    case 4:
                        EffectManager.sendUIEffect(_id, _key, _isReliable, _arg0, _arg1, _arg2, _arg3);
                        break;
                }
            }
            else
            {
                switch (_argCt)
                {
                    case 0:
                        EffectManager.sendUIEffect(_id, _key, _connection, _isReliable);
                        break;
                    case 1:
                        EffectManager.sendUIEffect(_id, _key, _connection, _isReliable, _arg0);
                        break;
                    case 2:
                        EffectManager.sendUIEffect(_id, _key, _connection, _isReliable, _arg0, _arg1);
                        break;
                    case 3:
                        EffectManager.sendUIEffect(_id, _key, _connection, _isReliable, _arg0, _arg1, _arg2);
                        break;
                    case 4:
                        EffectManager.sendUIEffect(_id, _key, _connection, _isReliable, _arg0, _arg1, _arg2, _arg3);
                        break;
                }
            }
        }
    }

    private class AddButtonPressedListenerState
    {
        private readonly EffectManager.EffectButtonClickedHandler _callback;
        public AddButtonPressedListenerState(EffectManager.EffectButtonClickedHandler callback)
        {
            _callback = callback;
        }

        public void Add()
        {
            EffectManager.onEffectButtonClicked += _callback;
        }
        public void Remove()
        {
            EffectManager.onEffectButtonClicked -= _callback;
        }
    }

    private class AddTextCommittedListenerState
    {
        private readonly EffectManager.EffectTextCommittedHandler _callback;
        public AddTextCommittedListenerState(EffectManager.EffectTextCommittedHandler callback)
        {
            _callback = callback;
        }

        public void Add()
        {
            EffectManager.onEffectTextCommitted += _callback;
        }
        public void Remove()
        {
            EffectManager.onEffectTextCommitted -= _callback;
        }
    }

    private class AddProviderDisconnectedListenerState
    {
        private readonly Provider.ServerDisconnected _callback;
        public AddProviderDisconnectedListenerState(Provider.ServerDisconnected callback)
        {
            _callback = callback;
        }

        public void Add()
        {
            Provider.onServerDisconnected += _callback;
        }
        public void Remove()
        {
            Provider.onServerDisconnected -= _callback;
        }
    }
}