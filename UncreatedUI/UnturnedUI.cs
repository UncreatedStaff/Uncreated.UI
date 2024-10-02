using Cysharp.Threading.Tasks;
using DanielWillett.ReflectionTools;
using Microsoft.Extensions.Logging;
using SDG.NetTransport;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging.Abstractions;
using Uncreated.Framework.UI.Reflection;

namespace Uncreated.Framework.UI;

/// <summary>
/// Container serving as a in-code representation of a Unity UI Effect.
/// </summary>
public class UnturnedUI : IDisposable
{
    private IAssetContainer? _container;
    private string _name;
    private int _disposed;
    private bool _waitingOnAssetLoad;
    protected internal readonly ILogger? Logger;
    internal readonly ILoggerFactory? Factory;
    private readonly List<UnturnedUIElement> _elements;
    private readonly string? _basePath;

    /// <summary>
    /// If the current configuration includes a valid asset or 16-bit Id.
    /// </summary>
    public bool HasAssetOrId { get; private set; }

    /// <summary>
    /// The current asset's Id, or 0.
    /// </summary>
    public ushort Id { get; private set; }

    /// <summary>
    /// The current asset's GUID, or 0.
    /// </summary>
    public Guid Guid { get; private set; }

    /// <summary>
    /// The current asset, or <see langword="null"/>.
    /// </summary>
    public EffectAsset? Asset { get; private set; }

    /// <summary>
    /// Are all UI write operations reliable?
    /// </summary>
    public bool IsReliable { get; }

    /// <summary>
    /// Is the initial sending of the UI reliable?
    /// </summary>
    public bool IsSendReliable { get; set; }

    /// <summary>
    /// Does this UI support elements defined in fields and properties?
    /// </summary>
    public bool HasElements { get; }

    /// <summary>
    /// Is debug logging enabled on this object?
    /// </summary>
    public bool DebugLogging { get; }

    /// <summary>
    /// All elements in this UI.
    /// </summary>
    public IReadOnlyList<UnturnedUIElement> Elements { get; }

    /// <summary>
    /// If the name has been changed from the default (asset name).
    /// </summary>
    public bool HasDefaultName { get; private set; } = true;

    /// <summary>
    /// Display name of the UI.
    /// </summary>
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            HasDefaultName = false;
        }
    }

    /// <summary>
    /// Key used to identify a single instance of this UI. -1 if this UI is keyless.
    /// </summary>
    public short Key { get; set; }
    private UnturnedUI(object? logger, bool hasElements, bool keyless, bool reliable, bool debugLogging)
    {
        Type type = GetType();

        bool isUnturnedUIObject = type == typeof(UnturnedUI);

        HasElements = hasElements && !isUnturnedUIObject;
        List<UnturnedUIElement> elements = new List<UnturnedUIElement>(hasElements ? 16 : 0);
        Elements = elements.AsReadOnly();
        _elements = elements;
        Key = keyless ? (short)-1 : UnturnedUIKeyPool.Claim();
        IsReliable = reliable;
        IsSendReliable = reliable;

        _name = type.Name;
        if (logger is not null and not NullLoggerFactory and not NullLogger)
        {
            DebugLogging = debugLogging;
            if (logger is ILoggerFactory factory)
            {
                Logger = factory.CreateLogger(_name);
                Factory = factory;
            }
            else
            {
                Logger = (ILogger)logger;
            }

            DebugLogging &= Logger != null;
        }

        _basePath = null;
        if (!isUnturnedUIObject && Attribute.GetCustomAttribute(type, typeof(UnturnedUIAttribute)) is UnturnedUIAttribute attr)
        {
            if (!string.IsNullOrEmpty(attr.DisplayName))
                Name = attr.DisplayName;
            if (attr.HasReliable)
                IsReliable = attr.Reliable;
            if (attr.HasHasElements)
                HasElements = attr.HasElements;
            _basePath = attr.BasePath;
        }

        if (!HasElements)
            return;

        UIElementDiscovery.LinkAllElements(this, elements);

        for (int i = 0; i < elements.Count; ++i)
        {
            SetupRelativeElementPath(elements[i]);
        }
    }

    public UnturnedUI(ushort defaultId, bool hasElements = true, bool keyless = false, bool reliable = true, bool debugLogging = false)
        : this(GlobalLogger.Instance, defaultId, hasElements, keyless, reliable, debugLogging) { }
    public UnturnedUI(ILogger logger, ushort defaultId, bool hasElements = true, bool keyless = false, bool reliable = true, bool debugLogging = false)
        : this(logger ?? throw new ArgumentNullException(nameof(logger)), hasElements, keyless, reliable, debugLogging)
    {
        LoadFromConfig(defaultId);
    }
    public UnturnedUI(ILoggerFactory factory, ushort defaultId, bool hasElements = true, bool keyless = false, bool reliable = true, bool debugLogging = false)
        : this(factory ?? throw new ArgumentNullException(nameof(factory)), hasElements, keyless, reliable, debugLogging)
    {
        LoadFromConfig(defaultId);
    }

    public UnturnedUI(Guid defaultGuid, bool hasElements = true, bool keyless = false, bool reliable = true, bool debugLogging = false)
        : this(GlobalLogger.Instance, defaultGuid, hasElements, keyless, reliable, debugLogging) { }
    public UnturnedUI(ILogger logger, Guid defaultGuid, bool hasElements = true, bool keyless = false, bool reliable = true, bool debugLogging = false)
        : this(logger ?? throw new ArgumentNullException(nameof(logger)), hasElements, keyless, reliable, debugLogging)
    {
        LoadFromConfig(defaultGuid);
    }
    public UnturnedUI(ILoggerFactory factory, Guid defaultGuid, bool hasElements = true, bool keyless = false, bool reliable = true, bool debugLogging = false)
        : this(factory ?? throw new ArgumentNullException(nameof(factory)), hasElements, keyless, reliable, debugLogging)
    {
        LoadFromConfig(defaultGuid);
    }

    public UnturnedUI(IAssetContainer assetContainer, bool hasElements = true, bool keyless = false, bool reliable = true, bool debugLogging = false)
        : this(GlobalLogger.Instance, assetContainer, hasElements, keyless, reliable, debugLogging) { }
    public UnturnedUI(ILogger logger, IAssetContainer assetContainer, bool hasElements = true, bool keyless = false, bool reliable = true, bool debugLogging = false)
        : this(logger ?? throw new ArgumentNullException(nameof(logger)), hasElements, keyless, reliable, debugLogging)
    {
        LoadFromConfig(assetContainer);
    }
    public UnturnedUI(ILoggerFactory factory, IAssetContainer assetContainer, bool hasElements = true, bool keyless = false, bool reliable = true, bool debugLogging = false)
        : this(factory ?? throw new ArgumentNullException(nameof(factory)), hasElements, keyless, reliable, debugLogging)
    {
        LoadFromConfig(assetContainer);
    }

    public UnturnedUI(EffectAsset? asset, bool hasElements = true, bool keyless = false, bool reliable = true, bool debugLogging = false)
        : this(GlobalLogger.Instance, asset, hasElements, keyless, reliable, debugLogging) { }
    public UnturnedUI(ILogger logger, EffectAsset? asset, bool hasElements = true, bool keyless = false, bool reliable = true, bool debugLogging = false)
        : this(logger ?? throw new ArgumentNullException(nameof(logger)), hasElements, keyless, reliable, debugLogging)
    {
        LoadFromConfig(asset);
    }
    public UnturnedUI(ILoggerFactory factory, EffectAsset? asset, bool hasElements = true, bool keyless = false, bool reliable = true, bool debugLogging = false)
        : this(factory ?? throw new ArgumentNullException(nameof(factory)), hasElements, keyless, reliable, debugLogging)
    {
        LoadFromConfig(asset);
    }

    /// <summary>
    /// Set the asset's GUID to <paramref name="guid"/>.
    /// </summary>
    public void LoadFromConfig(Guid guid)
    {
        if (Thread.CurrentThread.IsGameThread())
        {
            Guid = guid;
            Id = 0;
            Asset = Assets.find(guid) as EffectAsset;
            _container = null;
            LoadFromConfigIntl(false);
        }
        else
        {
            Guid guid2 = guid;
            UniTask.Create(async () =>
            {
                await UniTask.SwitchToMainThread();
                Guid = guid2;
                Id = 0;
                Asset = Assets.find(guid2) as EffectAsset;
                _container = null;
                LoadFromConfigIntl(false);
            });
        }
    }

    /// <summary>
    /// Set the asset's info to <paramref name="assetContainer"/>.
    /// </summary>
    public void LoadFromConfig(IAssetContainer assetContainer)
    {
        if (Thread.CurrentThread.IsGameThread())
        {
            Guid = assetContainer.Guid;
            Id = assetContainer.Id;
            Asset = assetContainer.Asset as EffectAsset;
            _container = assetContainer;
            LoadFromConfigIntl(false);
        }
        else
        {
            IAssetContainer container2 = assetContainer;
            UniTask.Create(async () =>
            {
                await UniTask.SwitchToMainThread();
                Guid = container2.Guid;
                Id = container2.Id;
                Asset = container2.Asset as EffectAsset;
                _container = container2;
                LoadFromConfigIntl(false);
            });
        }
    }

    /// <summary>
    /// Set the asset's ID to <paramref name="id"/>.
    /// </summary>
    public void LoadFromConfig(ushort id)
    {
        if (Thread.CurrentThread.IsGameThread())
        {
            Guid = default;
            Id = id;
            Asset = Assets.find(EAssetType.EFFECT, id) as EffectAsset;
            _container = null;
            LoadFromConfigIntl(false);
        }
        else
        {
            ushort id2 = id;
            UniTask.Create(async () =>
            {
                await UniTask.SwitchToMainThread();
                Guid = default;
                Id = id2;
                Asset = Assets.find(EAssetType.EFFECT, id2) as EffectAsset;
                _container = null;
                LoadFromConfigIntl(false);
            });
        }
    }

    /// <summary>
    /// Set the asset to <paramref name="asset"/>.
    /// </summary>
    public void LoadFromConfig(EffectAsset? asset)
    {
        if (Thread.CurrentThread.IsGameThread())
        {
            Asset = asset;
            if (asset == null)
            {
                Guid = default;
                Id = default;
            }
            else
            {
                Guid = asset.GUID;
                Id = asset.id;
            }
            _container = null;
            LoadFromConfigIntl(false);
        }
        else
        {
            EffectAsset? asset2 = asset;
            UniTask.Create(async () =>
            {
                await UniTask.SwitchToMainThread();
                Asset = asset2;
                if (asset2 == null)
                {
                    Guid = default;
                    Id = default;
                }
                else
                {
                    Guid = asset2.GUID;
                    Id = asset2.id;
                }
                _container = null;
                LoadFromConfigIntl(false);
            });
        }
    }

    /// <summary>
    /// Register an element after the base constructor ran.
    /// </summary>
    /// <remarks>This is not thread-safe and would usually only be ran in the child's constructor.</remarks>
    /// <exception cref="ArgumentNullException"/>
    protected void LateRegisterElement(UnturnedUIElement element)
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        if (_elements.Contains(element))
            return;

        _elements.Add(element);
        SetupRelativeElementPath(element);
        element.RegisterOwnerIntl(this, Factory);

        if (DebugLogging)
            Logger.LogInformation("[{0}] Late-registered 1 element of type {1}.", Name, Accessor.Formatter.Format(element.GetType()));
    }

    /// <summary>
    /// Register an element, pattern/preset object, or list/array of the following after the base constructor ran.
    /// </summary>
    /// <remarks>This is not thread-safe and would usually only be ran in the child's constructor.</remarks>
    /// <exception cref="ArgumentException"><paramref name="obj"/> is another <see cref="UnturnedUI"/>.</exception>
    /// <exception cref="ArgumentNullException"/>
    protected void LateRegisterElement(object obj)
    {
        if (obj is UnturnedUIElement uiElement)
        {
            LateRegisterElement(uiElement);
            return;
        }

        if (obj is UnturnedUI)
            throw new ArgumentException("Can not register another UI.", nameof(obj));

        if (obj == null)
            throw new ArgumentNullException(nameof(obj));

        int depth = 1;
        int pos = _elements.Count;
        UIElementDiscovery.DiscoverElements(Factory, Logger, obj, _elements, ref depth, DebugLogging, this);
        for (int i = pos; i < _elements.Count; ++i)
        {
            SetupRelativeElementPath(_elements[i]);
        }

        if (DebugLogging && pos != _elements.Count)
            Logger.LogInformation("[{0}] Late-registered {1} element(s) from {2}.", Name, _elements.Count - pos, Accessor.Formatter.Format(obj.GetType()));
    }

    private void SetupRelativeElementPath(UnturnedUIElement element)
    {
        ReadOnlySpan<char> pathSpan = element.Path;
        if (!string.IsNullOrEmpty(_basePath))
        {
            element.Path = UnturnedUIUtility.ResolveRelativePath(_basePath, pathSpan, assumeRelative: true);
        }
        else if (pathSpan.Length > 0)
        {
            element.Path = UnturnedUIUtility.ResolveRelativePath(default, pathSpan);
        }
    }
    private void LoadFromConfigIntl(bool assetsJustLoaded)
    {
        if (_container != null)
        {
            Id = _container.Id;
            Guid = _container.Guid;
            Asset = _container.Asset as EffectAsset;
        }

        if (Asset != null)
        {
            if (HasDefaultName)
                _name = Asset.FriendlyName;
            Id = Asset.id;
            Guid = Asset.GUID;
            if (Id == 0)
            {
                Logger.LogWarning("No id available, asset: {0}.", Asset.FriendlyName);
            }
        }
        else if (Id != 0)
        {
            if (HasDefaultName)
                _name = Id.ToString();
            if (!assetsJustLoaded && Assets.isLoading)
            {
                if (!_waitingOnAssetLoad)
                {
                    Level.onPrePreLevelLoaded += OnAssetsLoaded;
                    _waitingOnAssetLoad = true;
                }
            }
            else
            {
                Logger.LogWarning("No asset available, id: {0}.", Id);
            }
        }
        else if (Guid != default)
        {
            if (HasDefaultName)
                _name = Guid.ToString("N");

            HasAssetOrId = false;
            if (!assetsJustLoaded && Assets.isLoading)
            {
                if (!_waitingOnAssetLoad)
                {
                    Level.onPrePreLevelLoaded += OnAssetsLoaded;
                    _waitingOnAssetLoad = true;
                }
            }
            else
            {
                Logger.LogWarning("No asset or ID available, guid: {0}.", Guid);
            }
            return;
        }
        else
        {
            HasAssetOrId = false;
            Logger.LogWarning("No asset or ID available.");
            return;
        }

        HasAssetOrId = true;
    }

    private void OnAssetsLoaded(int lvlId)
    {
        if (lvlId != Level.BUILD_INDEX_GAME)
            return;

        Level.onPrePreLevelLoaded -= OnAssetsLoaded;
        _waitingOnAssetLoad = false;

        LoadFromConfigIntl(true);
    }

    /// <summary>
    /// Send this UI to a single player with no formatting arguments.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public virtual void SendToPlayer(SteamPlayer player)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));

        if (player.player.isActiveAndEnabled)
            SendToPlayerIntl(player.transportConnection);
    }

    /// <summary>
    /// Send this UI to a single player with no formatting arguments.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public virtual void SendToPlayer(Player player)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));

        if (player.isActiveAndEnabled)
            SendToPlayerIntl(player.channel.owner.transportConnection);
    }

    /// <summary>
    /// Send this UI to a single player with no formatting arguments.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public virtual void SendToPlayer(ITransportConnection connection)
    {
        if (connection == null)
            throw new ArgumentNullException(nameof(connection));

        SendToPlayerIntl(connection);
    }

    /// <summary>
    /// Send this UI to a single player with 1 formatting argument which replaces any <c>{0}</c> placeholders.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public virtual void SendToPlayer(SteamPlayer player, string arg0)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));

        if (player.player.isActiveAndEnabled)
            SendToPlayerIntl(player.transportConnection, arg0);
    }

    /// <summary>
    /// Send this UI to a single player with 1 formatting argument which replaces any <c>{0}</c> placeholders.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public virtual void SendToPlayer(Player player, string arg0)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));

        if (player.isActiveAndEnabled)
            SendToPlayerIntl(player.channel.owner.transportConnection, arg0);
    }

    /// <summary>
    /// Send this UI to a single player with 1 formatting argument which replaces any <c>{0}</c> placeholders.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public virtual void SendToPlayer(ITransportConnection connection, string arg0)
    {
        if (connection == null)
            throw new ArgumentNullException(nameof(connection));

        SendToPlayerIntl(connection, arg0);
    }

    /// <summary>
    /// Send this UI to a single player with 2 formatting arguments which replace any <c>{n}</c> placeholders.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public virtual void SendToPlayer(SteamPlayer player, string arg0, string arg1)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));

        if (player.player.isActiveAndEnabled)
            SendToPlayerIntl(player.transportConnection, arg0, arg1);
    }

    /// <summary>
    /// Send this UI to a single player with 2 formatting arguments which replace any <c>{n}</c> placeholders.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public virtual void SendToPlayer(Player player, string arg0, string arg1)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));

        if (player.isActiveAndEnabled)
            SendToPlayerIntl(player.channel.owner.transportConnection, arg0, arg1);
    }

    /// <summary>
    /// Send this UI to a single player with 2 formatting arguments which replace any <c>{n}</c> placeholders.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public virtual void SendToPlayer(ITransportConnection connection, string arg0, string arg1)
    {
        if (connection == null)
            throw new ArgumentNullException(nameof(connection));

        SendToPlayerIntl(connection, arg0, arg1);
    }

    /// <summary>
    /// Send this UI to a single player with 3 formatting arguments which replace any <c>{n}</c> placeholders.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public virtual void SendToPlayer(SteamPlayer player, string arg0, string arg1, string arg2)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));

        if (player.player.isActiveAndEnabled)
            SendToPlayerIntl(player.transportConnection, arg0, arg1, arg2);
    }

    /// <summary>
    /// Send this UI to a single player with 3 formatting arguments which replace any <c>{n}</c> placeholders.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public virtual void SendToPlayer(Player player, string arg0, string arg1, string arg2)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));

        if (player.isActiveAndEnabled)
            SendToPlayerIntl(player.channel.owner.transportConnection, arg0, arg1, arg2);
    }

    /// <summary>
    /// Send this UI to a single player with 3 formatting arguments which replace any <c>{n}</c> placeholders.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public virtual void SendToPlayer(ITransportConnection connection, string arg0, string arg1, string arg2)
    {
        if (connection == null)
            throw new ArgumentNullException(nameof(connection));

        SendToPlayerIntl(connection, arg0, arg1, arg2);
    }

    /// <summary>
    /// Send this UI to a single player with 4 formatting arguments which replace any <c>{n}</c> placeholders.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public virtual void SendToPlayer(SteamPlayer player, string arg0, string arg1, string arg2, string arg3)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));

        if (player.player.isActiveAndEnabled)
            SendToPlayerIntl(player.transportConnection, arg0, arg1, arg2, arg3);
    }

    /// <summary>
    /// Send this UI to a single player with 4 formatting arguments which replace any <c>{n}</c> placeholders.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public virtual void SendToPlayer(Player player, string arg0, string arg1, string arg2, string arg3)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));

        if (player.isActiveAndEnabled)
            SendToPlayerIntl(player.channel.owner.transportConnection, arg0, arg1, arg2, arg3);
    }

    /// <summary>
    /// Send this UI to a single player with 4 formatting arguments which replace any <c>{n}</c> placeholders.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public virtual void SendToPlayer(ITransportConnection connection, string arg0, string arg1, string arg2, string arg3)
    {
        if (connection == null)
            throw new ArgumentNullException(nameof(connection));

        SendToPlayerIntl(connection, arg0, arg1, arg2, arg3);
    }

    /// <summary>
    /// Clear this UI from a single player.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public virtual void ClearFromPlayer(SteamPlayer player)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));

        if (player.player.isActiveAndEnabled)
            ClearFromPlayerIntl(player.transportConnection);
    }

    /// <summary>
    /// Clear this UI from a single player.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public virtual void ClearFromPlayer(Player player)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));

        if (player.isActiveAndEnabled)
            ClearFromPlayerIntl(player.channel.owner.transportConnection);
    }

    /// <summary>
    /// Clear this UI from a single player.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public virtual void ClearFromPlayer(ITransportConnection connection)
    {
        if (connection == null)
            throw new ArgumentNullException(nameof(connection));

        ClearFromPlayerIntl(connection);
    }

    /// <summary>
    /// Sends this UI to all online players.
    /// </summary>
    public virtual void SendToAllPlayers()
    {
        if (!HasAssetOrId)
        {
            Logger.LogWarning("[{0}] No asset or id.", Name);
            return;
        }

        if (Thread.CurrentThread.IsGameThread())
        {
            EffectManager.sendUIEffect(Id, Key, IsReliable || IsSendReliable);
        }
        else
        {
            UniTask.Create(async () =>
            {
                await UniTask.SwitchToMainThread();
                EffectManager.sendUIEffect(Id, Key, IsReliable || IsSendReliable);
            });
        }

        if (DebugLogging)
            Logger.LogInformation("[{0}] Sent to all players.", Name);
    }

    /// <summary>
    /// Clears this UI from all online players.
    /// </summary>
    public virtual void ClearFromAllPlayers()
    {
        if (!HasAssetOrId)
        {
            Logger.LogWarning("[{0}] No asset or id.", Name);
            return;
        }

        if (Thread.CurrentThread.IsGameThread())
        {
            EffectManager.ClearEffectByID_AllPlayers(Id);
        }
        else
        {
            UniTask.Create(async () =>
            {
                await UniTask.SwitchToMainThread();
                EffectManager.ClearEffectByID_AllPlayers(Id);
            });
        }

        if (DebugLogging)
            Logger.LogInformation("[{0}] Cleared from all players.", Name);
    }
    private void SendToPlayerIntl(ITransportConnection connection)
    {
        if (!HasAssetOrId)
        {
            Logger.LogWarning("[{0}] No asset or id.", Name);
            return;
        }

        if (Thread.CurrentThread.IsGameThread())
        {
            EffectManager.sendUIEffect(Id, Key, connection, IsReliable || IsSendReliable);
        }
        else
        {
            ITransportConnection c2 = connection;
            UniTask.Create(async () =>
            {
                await UniTask.SwitchToMainThread();
                EffectManager.sendUIEffect(Id, Key, c2, IsReliable || IsSendReliable);
            });
        }

        if (DebugLogging)
            Logger.LogInformation("[{0}] Sent to {1}.", Name, connection.GetAddressString(true));
    }
    private void SendToPlayerIntl(ITransportConnection connection, string arg0)
    {
        if (!HasAssetOrId)
        {
            Logger.LogWarning("[{0}] No asset or id.", Name);
            return;
        }

        if (Thread.CurrentThread.IsGameThread())
        {
            EffectManager.sendUIEffect(Id, Key, connection, IsReliable || IsSendReliable, arg0);
        }
        else
        {
            ITransportConnection c2 = connection;
            string arg02 = arg0;
            UniTask.Create(async () =>
            {
                await UniTask.SwitchToMainThread();
                EffectManager.sendUIEffect(Id, Key, c2, IsReliable || IsSendReliable, arg02);
            });
        }

        if (DebugLogging)
            Logger.LogInformation("[{0}] Sent to {1}, arg: {2}.", Name, connection.GetAddressString(true), arg0);
    }
    private void SendToPlayerIntl(ITransportConnection connection, string arg0, string arg1)
    {
        if (!HasAssetOrId)
        {
            Logger.LogWarning("[{0}] No asset or id.", Name);
            return;
        }

        if (Thread.CurrentThread.IsGameThread())
        {
            EffectManager.sendUIEffect(Id, Key, connection, IsReliable || IsSendReliable, arg0, arg1);
        }
        else
        {
            ITransportConnection c2 = connection;
            string arg02 = arg0;
            string arg12 = arg1;
            UniTask.Create(async () =>
            {
                await UniTask.SwitchToMainThread();
                EffectManager.sendUIEffect(Id, Key, c2, IsReliable || IsSendReliable, arg02, arg12);
            });
        }

        if (DebugLogging)
            Logger.LogInformation("[{0}] Sent to {1}, args: {{0}} = {2}, {{1}} = {3}.", Name, connection.GetAddressString(true), arg0, arg1);
    }
    private void SendToPlayerIntl(ITransportConnection connection, string arg0, string arg1, string arg2)
    {
        if (!HasAssetOrId)
        {
            Logger.LogWarning("[{0}] No asset or id.", Name);
            return;
        }

        if (Thread.CurrentThread.IsGameThread())
        {
            EffectManager.sendUIEffect(Id, Key, connection, IsReliable || IsSendReliable, arg0, arg1, arg2);
        }
        else
        {
            ITransportConnection c2 = connection;
            string arg02 = arg0;
            string arg12 = arg1;
            string arg22 = arg2;
            UniTask.Create(async () =>
            {
                await UniTask.SwitchToMainThread();
                EffectManager.sendUIEffect(Id, Key, c2, IsReliable || IsSendReliable, arg02, arg12, arg22);
            });
        }

        if (DebugLogging)
            Logger.LogInformation("[{0}] Sent to {1}, args: {{0}} = {2}, {{1}} = {3}, {{2}} = {4}.", Name, connection.GetAddressString(true), arg0, arg1, arg2);
    }
    private void SendToPlayerIntl(ITransportConnection connection, string arg0, string arg1, string arg2, string arg3)
    {
        if (!HasAssetOrId)
        {
            Logger.LogWarning("[{0}] No asset or id.", Name);
            return;
        }

        if (Thread.CurrentThread.IsGameThread())
        {
            EffectManager.sendUIEffect(Id, Key, connection, IsReliable || IsSendReliable, arg0, arg1, arg2, arg3);
        }
        else
        {
            ITransportConnection c2 = connection;
            string arg02 = arg0;
            string arg12 = arg1;
            string arg22 = arg2;
            string arg32 = arg3;
            UniTask.Create(async () =>
            {
                await UniTask.SwitchToMainThread();
                EffectManager.sendUIEffect(Id, Key, c2, IsReliable || IsSendReliable, arg02, arg12, arg22, arg32);
            });
        }

        if (DebugLogging)
            Logger.LogInformation("[{0}] Sent to {1}, args: {{0}} = {2}, {{1}} = {3}, {{2}} = {4}, {{3}} = {5}.", Name, connection.GetAddressString(true), arg0, arg1, arg2, arg3);
    }
    private void ClearFromPlayerIntl(ITransportConnection connection)
    {
        if (!HasAssetOrId)
        {
            Logger.LogWarning("[{0}] No asset or id.", Name);
            return;
        }

        if (Thread.CurrentThread.IsGameThread())
        {
            EffectManager.askEffectClearByID(Id, connection);
        }
        else
        {
            ITransportConnection c2 = connection;
            UniTask.Create(async () =>
            {
                await UniTask.SwitchToMainThread();
                EffectManager.askEffectClearByID(Id, c2);
            });
        }

        if (DebugLogging)
            Logger.LogInformation("[{0}] Cleared from {1}.", Name, connection.GetAddressString(true));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
    }
    private void Dispose(bool disposing)
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        IUnturnedUIDataSource? src = UnturnedUIDataSource.Instance;

        if (src != null)
        {
            if (src.RequiresMainThread && !Thread.CurrentThread.IsGameThread())
            {
                IUnturnedUIDataSource src2 = src;
                List<UnturnedUIElement> elements = Elements.ToList();
                UniTask.Create(async () =>
                {
                    await UniTask.SwitchToMainThread();
                    IntlDispose(elements, src2);
                });
            }
            else
            {
                IntlDispose(Elements, src);
            }
        }

        if (disposing)
            GC.SuppressFinalize(this);
    }
    private void IntlDispose(IReadOnlyList<UnturnedUIElement> elements, IUnturnedUIDataSource src)
    {
        if (DebugLogging)
            Logger.LogInformation("[{0}] Deregistering {1} elements.", Name, elements.Count);

        src.RemoveOwner(this);
        for (int i = 0; i < elements.Count; ++i)
        {
            UnturnedUIElement element = elements[i];
            src.RemoveElement(element);
            element.DeregisterOwnerIntl();
            if (element is IDisposable disp)
                disp.Dispose();
        }
    }
    ~UnturnedUI()
    {
        Dispose(false);
    }
}