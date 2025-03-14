using DanielWillett.ReflectionTools;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SDG.NetTransport;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Uncreated.Framework.UI.Data;
using Uncreated.Framework.UI.Reflection;

namespace Uncreated.Framework.UI;

/// <summary>
/// Container serving as a in-code representation of a Unity UI Effect.
/// </summary>
public class UnturnedUI : IDisposable
{
    private static class StaticKeys
    {
        public static readonly ConcurrentDictionary<Type, short> StaticKeysDictIntl = new ConcurrentDictionary<Type, short>();
        static StaticKeys() { }
    }

    private IAssetContainer? _container;
    private string _name;
    private int _disposed;
    private bool _waitingOnAssetLoad;

    protected internal ILogger? Logger;
    protected internal readonly ILoggerFactory? LoggerFactory;

    private readonly List<UnturnedUIElement> _elements;
    private readonly string? _basePath;

    /// <summary>
    /// Get a lazily cached <see cref="ILogger"/> object for this element. Will never be <see langword="null"/>.
    /// </summary>
    protected internal ILogger GetLogger()
    {
        if (Logger != null)
            return Logger;

        if (LoggerFactory == null)
            return NullLogger.Instance;

        Logger = LoggerFactory.CreateLogger(Name);
        return Logger;
    }

    /// <summary>
    /// If the current configuration includes a valid asset or 16-bit Id.
    /// </summary>
    [Obsolete("IDs are no longer used to send UIs.")]
    public bool HasAssetOrId { get; private set; }

    /// <summary>
    /// If the current configuration includes a valid asset.
    /// </summary>
    public bool HasAsset => Asset != null;

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
    private UnturnedUI(object? logger, UnturnedUIOptions options)
        : this (logger,
                hasElements:    (options & UnturnedUIOptions.HasElements) != 0,
                keyless:        (options & UnturnedUIOptions.Keyless) != 0,
                reliable:       (options & UnturnedUIOptions.Reliable) != 0,
                debugLogging:   (options & UnturnedUIOptions.DebugLogging) != 0,
                staticKey:      (options & UnturnedUIOptions.StaticKey) != 0)
    {

    }

    private UnturnedUI(object? logger, bool hasElements, bool keyless, bool reliable, bool debugLogging, bool staticKey)
    {
        staticKey &= !keyless;
        Type type = GetType();

        bool isUnturnedUIObject = type == typeof(UnturnedUI);

        HasElements = hasElements && !isUnturnedUIObject;
        List<UnturnedUIElement> elements = new List<UnturnedUIElement>(hasElements ? 16 : 0);
        Elements = elements.AsReadOnly();
        _elements = elements;
        Key = keyless ? (short)-1 : (staticKey ? ClaimStaticKey(GetType()) : UnturnedUIKeyPool.Claim());
        IsReliable = reliable;
        IsSendReliable = reliable;

        _name = type.Name;
        if (logger is not null and not NullLoggerFactory and not NullLogger)
        {
            DebugLogging = debugLogging;
            if (logger is ILoggerFactory factory)
            {
                LoggerFactory = factory;
            }
            else
            {
                Logger = logger as ILogger;
            }

            DebugLogging &= Logger != null || LoggerFactory != null;
        }

        _basePath = null;
        if (!isUnturnedUIObject && Attribute.GetCustomAttribute(type, typeof(UnturnedUIAttribute), true) is UnturnedUIAttribute attr)
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

        List<object> circularReferenceStack = new List<object>(3);
        UIElementDiscovery.LinkAllElements(this, elements, circularReferenceStack);

        for (int i = 0; i < elements.Count; ++i)
        {
            SetupRelativeElementPath(elements[i]);
        }
    }

    [Obsolete("Use GUIDs over IDs.")]
    public UnturnedUI(ushort defaultId, bool hasElements = true, bool keyless = false, bool reliable = true, bool debugLogging = false, bool staticKey = false)
        : this(GlobalLogger.Instance, defaultId, hasElements, keyless, reliable, debugLogging, staticKey) { }

    [Obsolete("Use GUIDs over IDs.")]
    public UnturnedUI(ILogger logger, ushort defaultId, bool hasElements = true, bool keyless = false, bool reliable = true, bool debugLogging = false, bool staticKey = false)
        : this(logger ?? throw new ArgumentNullException(nameof(logger)), hasElements, keyless, reliable, debugLogging, staticKey)
    {
        LoadFromConfig(defaultId);
    }

    [Obsolete("Use GUIDs over IDs.")]
    public UnturnedUI(ILoggerFactory factory, ushort defaultId, bool hasElements = true, bool keyless = false, bool reliable = true, bool debugLogging = false, bool staticKey = false)
        : this(factory ?? throw new ArgumentNullException(nameof(factory)), hasElements, keyless, reliable, debugLogging, staticKey)
    {
        LoadFromConfig(defaultId);
    }

    [Obsolete("Use GUIDs over IDs.")]
    public UnturnedUI(ushort defaultId, UnturnedUIOptions options)
        : this(GlobalLogger.Instance, defaultId, options) { }

    [Obsolete("Use GUIDs over IDs.")]
    public UnturnedUI(ILogger logger, ushort defaultId, UnturnedUIOptions options)
        : this(logger ?? throw new ArgumentNullException(nameof(logger)), options)
    {
        LoadFromConfig(defaultId);
    }

    [Obsolete("Use GUIDs over IDs.")]
    public UnturnedUI(ILoggerFactory factory, ushort defaultId, UnturnedUIOptions options)
        : this(factory ?? throw new ArgumentNullException(nameof(factory)), options)
    {
        LoadFromConfig(defaultId);
    }

    public UnturnedUI(Guid defaultGuid, bool hasElements = true, bool keyless = false, bool reliable = true, bool debugLogging = false, bool staticKey = false)
        : this(GlobalLogger.Instance, defaultGuid, hasElements, keyless, reliable, debugLogging, staticKey) { }
    public UnturnedUI(ILogger logger, Guid defaultGuid, bool hasElements = true, bool keyless = false, bool reliable = true, bool debugLogging = false, bool staticKey = false)
        : this(logger ?? throw new ArgumentNullException(nameof(logger)), hasElements, keyless, reliable, debugLogging, staticKey)
    {
        LoadFromConfig(defaultGuid);
    }
    public UnturnedUI(ILoggerFactory factory, Guid defaultGuid, bool hasElements = true, bool keyless = false, bool reliable = true, bool debugLogging = false, bool staticKey = false)
        : this(factory ?? throw new ArgumentNullException(nameof(factory)), hasElements, keyless, reliable, debugLogging, staticKey)
    {
        LoadFromConfig(defaultGuid);
    }

    public UnturnedUI(IAssetContainer assetContainer, bool hasElements = true, bool keyless = false, bool reliable = true, bool debugLogging = false, bool staticKey = false)
        : this(GlobalLogger.Instance, assetContainer, hasElements, keyless, reliable, debugLogging, staticKey) { }
    public UnturnedUI(ILogger logger, IAssetContainer assetContainer, bool hasElements = true, bool keyless = false, bool reliable = true, bool debugLogging = false, bool staticKey = false)
        : this(logger ?? throw new ArgumentNullException(nameof(logger)), hasElements, keyless, reliable, debugLogging, staticKey)
    {
        LoadFromConfig(assetContainer);
    }
    public UnturnedUI(ILoggerFactory factory, IAssetContainer assetContainer, bool hasElements = true, bool keyless = false, bool reliable = true, bool debugLogging = false, bool staticKey = false)
        : this(factory ?? throw new ArgumentNullException(nameof(factory)), hasElements, keyless, reliable, debugLogging, staticKey)
    {
        LoadFromConfig(assetContainer);
    }

    public UnturnedUI(EffectAsset? asset, bool hasElements = true, bool keyless = false, bool reliable = true, bool debugLogging = false, bool staticKey = false)
        : this(GlobalLogger.Instance, asset, hasElements, keyless, reliable, debugLogging, staticKey) { }
    public UnturnedUI(ILogger logger, EffectAsset? asset, bool hasElements = true, bool keyless = false, bool reliable = true, bool debugLogging = false, bool staticKey = false)
        : this(logger ?? throw new ArgumentNullException(nameof(logger)), hasElements, keyless, reliable, debugLogging, staticKey)
    {
        LoadFromConfig(asset);
    }
    public UnturnedUI(ILoggerFactory factory, EffectAsset? asset, bool hasElements = true, bool keyless = false, bool reliable = true, bool debugLogging = false, bool staticKey = false)
        : this(factory ?? throw new ArgumentNullException(nameof(factory)), hasElements, keyless, reliable, debugLogging, staticKey)
    {
        LoadFromConfig(asset);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Use GUIDs over IDs.")]
    public UnturnedUI(ushort defaultId, bool hasElements, bool keyless, bool reliable, bool debugLogging)
        : this(GlobalLogger.Instance, defaultId, hasElements, keyless, reliable, debugLogging, false) { }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Use GUIDs over IDs.")]
    public UnturnedUI(ILogger logger, ushort defaultId, bool hasElements, bool keyless, bool reliable, bool debugLogging)
        : this(logger ?? throw new ArgumentNullException(nameof(logger)), hasElements, keyless, reliable, debugLogging, false)
    {
        LoadFromConfig(defaultId);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Use GUIDs over IDs.")]
    public UnturnedUI(ILoggerFactory factory, ushort defaultId, bool hasElements, bool keyless, bool reliable, bool debugLogging)
        : this(factory ?? throw new ArgumentNullException(nameof(factory)), hasElements, keyless, reliable, debugLogging, false)
    {
        LoadFromConfig(defaultId);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public UnturnedUI(Guid defaultGuid, bool hasElements, bool keyless, bool reliable, bool debugLogging)
        : this(GlobalLogger.Instance, defaultGuid, hasElements, keyless, reliable, debugLogging, false) { }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public UnturnedUI(ILogger logger, Guid defaultGuid, bool hasElements, bool keyless, bool reliable, bool debugLogging)
        : this(logger ?? throw new ArgumentNullException(nameof(logger)), hasElements, keyless, reliable, debugLogging, false)
    {
        LoadFromConfig(defaultGuid);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public UnturnedUI(ILoggerFactory factory, Guid defaultGuid, bool hasElements, bool keyless, bool reliable, bool debugLogging)
        : this(factory ?? throw new ArgumentNullException(nameof(factory)), hasElements, keyless, reliable, debugLogging, false)
    {
        LoadFromConfig(defaultGuid);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public UnturnedUI(IAssetContainer assetContainer, bool hasElements, bool keyless, bool reliable, bool debugLogging)
        : this(GlobalLogger.Instance, assetContainer, hasElements, keyless, reliable, debugLogging, false) { }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public UnturnedUI(ILogger logger, IAssetContainer assetContainer, bool hasElements, bool keyless, bool reliable, bool debugLogging)
        : this(logger ?? throw new ArgumentNullException(nameof(logger)), hasElements, keyless, reliable, debugLogging, false)
    {
        LoadFromConfig(assetContainer);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public UnturnedUI(ILoggerFactory factory, IAssetContainer assetContainer, bool hasElements, bool keyless, bool reliable, bool debugLogging)
        : this(factory ?? throw new ArgumentNullException(nameof(factory)), hasElements, keyless, reliable, debugLogging, false)
    {
        LoadFromConfig(assetContainer);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public UnturnedUI(EffectAsset? asset, bool hasElements, bool keyless, bool reliable, bool debugLogging)
        : this(GlobalLogger.Instance, asset, hasElements, keyless, reliable, debugLogging, false) { }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public UnturnedUI(ILogger logger, EffectAsset? asset, bool hasElements, bool keyless, bool reliable, bool debugLogging)
        : this(logger ?? throw new ArgumentNullException(nameof(logger)), hasElements, keyless, reliable, debugLogging, false)
    {
        LoadFromConfig(asset);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public UnturnedUI(ILoggerFactory factory, EffectAsset? asset, bool hasElements, bool keyless, bool reliable, bool debugLogging)
        : this(factory ?? throw new ArgumentNullException(nameof(factory)), hasElements, keyless, reliable, debugLogging, false)
    {
        LoadFromConfig(asset);
    }

    private static short ClaimStaticKey(Type type)
    {
        return StaticKeys.StaticKeysDictIntl.GetOrAdd(type, static _ => UnturnedUIKeyPool.Claim());
    }

    /// <summary>
    /// Set the asset's GUID to <paramref name="guid"/>.
    /// </summary>
    public void LoadFromConfig(Guid guid)
    {
        if (UnturnedUIProvider.Instance.IsValidThread())
        {
            Guid = guid;
            Id = 0;
            Asset = UnturnedUIProvider.Instance.GetEffectAsset(guid);
            _container = null;
            LoadFromConfigIntl(false);
        }
        else
        {
            Guid guid2 = guid;
            UnturnedUIProvider.Instance.DispatchToValidThread(() =>
            {
                Guid = guid2;
                Id = 0;
                Asset = UnturnedUIProvider.Instance.GetEffectAsset(guid2);
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
        if (UnturnedUIProvider.Instance.IsValidThread())
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
            UnturnedUIProvider.Instance.DispatchToValidThread(() =>
            {
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
    [Obsolete("Use GUIDs over IDs.")]
    public void LoadFromConfig(ushort id)
    {
        if (UnturnedUIProvider.Instance.IsValidThread())
        {
            Guid = default;
            Id = id;
            Asset = UnturnedUIProvider.Instance.GetEffectAsset(id);
            _container = null;
            LoadFromConfigIntl(false);
        }
        else
        {
            ushort id2 = id;
            UnturnedUIProvider.Instance.DispatchToValidThread(() =>
            {
                Guid = default;
                Id = id2;
                Asset = UnturnedUIProvider.Instance.GetEffectAsset(id2);
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
        if (UnturnedUIProvider.Instance.IsValidThread())
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
            UnturnedUIProvider.Instance.DispatchToValidThread(() =>
            {
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
    /// Add new UI data.
    /// </summary>
    /// <remarks>Thread-safe.</remarks>
    /// <exception cref="InvalidOperationException"><paramref name="data"/> isn't made for this specific UI.</exception>
    public void AddData(IUnturnedUIData data)
    {
        if (data.Element == null || data.Owner != this)
            throw new InvalidOperationException(Properties.Resources.Exception_UnrelatedUIDataType);

        UnturnedUIDataSource.AddData(data);
    }

    /// <summary>
    /// Get the data, if it exists, for a given <paramref name="player"/> and this UI, otheewise create and add it
    /// </summary>
    /// <exception cref="NotSupportedException">Not ran on main thread if <see cref="IUnturnedUIDataSource.RequiresMainThread"/> is <see langword="true"/>.</exception>
    public TData GetOrAddData<TData>(CSteamID player, Func<CSteamID, TData> func) where TData : class, IUnturnedUIData
    {
        IUnturnedUIDataSource instance = UnturnedUIDataSource.Instance;
        if (instance.RequiresMainThread)
            UnturnedUIProvider.Instance.AssertValidThread();

        TData? data = instance.GetData<TData>(player, this);
        if (data == null)
        {
            data = func(player);
            if (data.Player.m_SteamID != player.m_SteamID || data.Element != null || data.Owner != this)
                throw new InvalidOperationException(Properties.Resources.Exception_InconsistantPlayerFromCreatedDataUI);
            instance.AddData(data);
        }

        return data;
    }

    /// <summary>
    /// Get the data, if it exists, for a given <paramref name="player"/> and this UI where the element is <see langword="null"/>.
    /// </summary>
    /// <exception cref="NotSupportedException">Not ran on main thread if <see cref="IUnturnedUIDataSource.RequiresMainThread"/> is <see langword="true"/>.</exception>
    public TData? GetData<TData>(CSteamID player) where TData : class, IUnturnedUIData
    {
        return UnturnedUIDataSource.GetData<TData>(player, this);
    }

    /// <summary>
    /// Enumerate through all data linked to this this UI.
    /// </summary>
    /// <exception cref="NotSupportedException">Not ran on main thread if <see cref="IUnturnedUIDataSource.RequiresMainThread"/> is <see langword="true"/>.</exception>
    public IEnumerable<IUnturnedUIData> EnumerateData()
    {
        return UnturnedUIDataSource.EnumerateData(this);
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

        lock (_elements)
        {
            if (_elements.Contains(element))
                return;

            _elements.Add(element);
            SetupRelativeElementPath(element);
            element.RegisterOwnerIntl(this);
        }

        if (DebugLogging)
            GetLogger().LogInformation(Properties.Resources.Log_LateRegisteredPrimitive, Name, element);
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
            throw new ArgumentException(Properties.Resources.Exception_LateRegisterUnturnedUI, nameof(obj));

        if (obj == null)
            throw new ArgumentNullException(nameof(obj));

        int depth = 1;
        lock (_elements)
        {
            int pos = _elements.Count;
            List<object> circularReferenceStack = new List<object>(2);
            UIElementDiscovery.DiscoverElements(GetLogger(), obj, _elements, ref depth, DebugLogging, this, circularReferenceStack);
            for (int i = pos; i < _elements.Count; ++i)
            {
                SetupRelativeElementPath(_elements[i]);
            }

            if (DebugLogging && pos != _elements.Count)
                GetLogger().LogInformation(Properties.Resources.Log_LateRegisteredOther, Name, _elements.Count - pos, Accessor.Formatter.Format(obj.GetType()));
        }
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
                GetLogger().LogWarning(Properties.Resources.Log_UnableToFindIdFromAsset, Name, Asset.FriendlyName);
            }
        }
        else if (Id != 0)
        {
            if (HasDefaultName)
                _name = Id.ToString();
            if (!assetsJustLoaded && UnturnedUIProvider.Instance.AreAssetsStillLoading)
            {
                SubLevelLoaded();
            }
            else
            {
                GetLogger().LogWarning(Properties.Resources.Log_UnableToFindAssetFromId, Name, Id);
            }
        }
        else if (Guid != default)
        {
            if (HasDefaultName)
                _name = Guid.ToString("N");

#pragma warning disable CS0618 // Type or member is obsolete
            HasAssetOrId = false;
#pragma warning restore CS0618
            if (!assetsJustLoaded && UnturnedUIProvider.Instance.AreAssetsStillLoading)
            {
                SubLevelLoaded();
            }
            else
            {
                GetLogger().LogWarning(Properties.Resources.Log_UnableToFindAssetFromGuid, Name, Guid);
            }
            return;
        }
        else
        {
#pragma warning disable CS0618
            HasAssetOrId = false;
#pragma warning restore CS0618
            GetLogger().LogWarning(Properties.Resources.Log_NoAssetSupplied, Name);
            return;
        }

#pragma warning disable CS0618
        HasAssetOrId = true;
#pragma warning restore CS0618
    }


    [MethodImpl(MethodImplOptions.NoInlining)]
    private void SubLevelLoaded()
    {
        if (_waitingOnAssetLoad)
            return;

        Level.onPrePreLevelLoaded += OnAssetsLoaded;
        _waitingOnAssetLoad = true;
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
        if (!HasAsset)
        {
            GetLogger().LogWarning(Properties.Resources.Log_NoAssetSupplied, Name);
            return;
        }

        UnturnedUIProvider.Instance.SendUIGlobal(Asset!, Key, IsReliable || IsSendReliable);

        if (DebugLogging)
            GetLogger().LogInformation(Properties.Resources.Log_SendToAllPlayers_0, Name);
    }

    /// <summary>
    /// Sends this UI to all online players with 1 formatting argument which replaces any <c>{0}</c> placeholders.
    /// </summary>
    public virtual void SendToAllPlayers(string arg0)
    {
        if (!HasAsset)
        {
            GetLogger().LogWarning(Properties.Resources.Log_NoAssetSupplied, Name);
            return;
        }

        UnturnedUIProvider.Instance.SendUIGlobal(Asset!, Key, IsReliable || IsSendReliable, arg0);

        if (DebugLogging)
            GetLogger().LogInformation(Properties.Resources.Log_SendToAllPlayers_1, Name, arg0);
    }

    /// <summary>
    /// Sends this UI to all online players with 2 formatting arguments which replace any <c>{n}</c> placeholders.
    /// </summary>
    public virtual void SendToAllPlayers(string arg0, string arg1)
    {
        if (!HasAsset)
        {
            GetLogger().LogWarning(Properties.Resources.Log_NoAssetSupplied, Name);
            return;
        }

        UnturnedUIProvider.Instance.SendUIGlobal(Asset!, Key, IsReliable || IsSendReliable, arg0, arg1);

        if (DebugLogging)
            GetLogger().LogInformation(Properties.Resources.Log_SendToAllPlayers_2, Name, arg0, arg1);
    }

    /// <summary>
    /// Sends this UI to all online players with 3 formatting arguments which replace any <c>{n}</c> placeholders.
    /// </summary>
    public virtual void SendToAllPlayers(string arg0, string arg1, string arg2)
    {
        if (!HasAsset)
        {
            GetLogger().LogWarning(Properties.Resources.Log_NoAssetSupplied, Name);
            return;
        }

        UnturnedUIProvider.Instance.SendUIGlobal(Asset!, Key, IsReliable || IsSendReliable, arg0, arg1, arg2);

        if (DebugLogging)
            GetLogger().LogInformation(Properties.Resources.Log_SendToAllPlayers_3, Name, arg0, arg1, arg2);
    }

    /// <summary>
    /// Sends this UI to all online players with 4 formatting arguments which replace any <c>{n}</c> placeholders.
    /// </summary>
    public virtual void SendToAllPlayers(string arg0, string arg1, string arg2, string arg3)
    {
        if (!HasAsset)
        {
            GetLogger().LogWarning(Properties.Resources.Log_NoAssetSupplied, Name);
            return;
        }

        UnturnedUIProvider.Instance.SendUIGlobal(Asset!, Key, IsReliable || IsSendReliable, arg0, arg1, arg2, arg3);

        if (DebugLogging)
            GetLogger().LogInformation(Properties.Resources.Log_SendToAllPlayers_4, Name, arg0, arg1, arg2, arg3);
    }

    /// <summary>
    /// Clears this UI from all online players.
    /// </summary>
    public virtual void ClearFromAllPlayers()
    {
        if (!HasAsset)
        {
            GetLogger().LogWarning(Properties.Resources.Log_NoAssetSupplied, Name);
            return;
        }

        UnturnedUIProvider.Instance.ClearGlobal(Asset!);

        if (DebugLogging)
            GetLogger().LogInformation(Properties.Resources.Log_ClearFromAllPlayers, Name);
    }

    private void SendToPlayerIntl(ITransportConnection connection)
    {
        if (!HasAsset)
        {
            GetLogger().LogWarning(Properties.Resources.Log_NoAssetSupplied, Name);
            return;
        }

        UnturnedUIProvider.Instance.SendUI(Asset!, Key, connection, IsReliable || IsSendReliable);

        if (DebugLogging)
            GetLogger().LogInformation(Properties.Resources.Log_SendToPlayer_0, Name, connection.GetAddressString(true));
    }

    private void SendToPlayerIntl(ITransportConnection connection, string arg0)
    {
        if (!HasAsset)
        {
            GetLogger().LogWarning(Properties.Resources.Log_NoAssetSupplied, Name);
            return;
        }

        UnturnedUIProvider.Instance.SendUI(Asset!, Key, connection, IsReliable || IsSendReliable, arg0);

        if (DebugLogging)
            GetLogger().LogInformation(Properties.Resources.Log_SendToPlayer_1, Name, connection.GetAddressString(true), arg0);
    }

    private void SendToPlayerIntl(ITransportConnection connection, string arg0, string arg1)
    {
        if (!HasAsset)
        {
            GetLogger().LogWarning(Properties.Resources.Log_NoAssetSupplied, Name);
            return;
        }

        UnturnedUIProvider.Instance.SendUI(Asset!, Key, connection, IsReliable || IsSendReliable, arg0, arg1);

        if (DebugLogging)
            GetLogger().LogInformation(Properties.Resources.Log_SendToPlayer_2, Name, connection.GetAddressString(true), arg0, arg1);
    }

    private void SendToPlayerIntl(ITransportConnection connection, string arg0, string arg1, string arg2)
    {
        if (!HasAsset)
        {
            GetLogger().LogWarning(Properties.Resources.Log_NoAssetSupplied, Name);
            return;
        }

        UnturnedUIProvider.Instance.SendUI(Asset!, Key, connection, IsReliable || IsSendReliable, arg0, arg1, arg2);

        if (DebugLogging)
            GetLogger().LogInformation(Properties.Resources.Log_SendToPlayer_3, Name, connection.GetAddressString(true), arg0, arg1, arg2);
    }

    private void SendToPlayerIntl(ITransportConnection connection, string arg0, string arg1, string arg2, string arg3)
    {
        if (!HasAsset)
        {
            GetLogger().LogWarning(Properties.Resources.Log_NoAssetSupplied, Name);
            return;
        }

        UnturnedUIProvider.Instance.SendUI(Asset!, Key, connection, IsReliable || IsSendReliable, arg0, arg1, arg2, arg3);

        if (DebugLogging)
            GetLogger().LogInformation(Properties.Resources.Log_SendToPlayer_4, Name, connection.GetAddressString(true), arg0, arg1, arg2, arg3);
    }

    private void ClearFromPlayerIntl(ITransportConnection connection)
    {
        if (!HasAsset)
        {
            GetLogger().LogWarning(Properties.Resources.Log_NoAssetSupplied, Name);
            return;
        }

        UnturnedUIProvider.Instance.Clear(Asset!, connection);

        if (DebugLogging)
            GetLogger().LogInformation(Properties.Resources.Log_ClearFromPlayer, Name, connection.GetAddressString(true));
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

        if (disposing) GC.SuppressFinalize(this);

        IUnturnedUIDataSource src = UnturnedUIDataSource.Instance;

        if (src is { RequiresMainThread: true } && !UnturnedUIProvider.Instance.IsValidThread())
        {
            IUnturnedUIDataSource src2 = src;
            lock (_elements)
            {
                List<UnturnedUIElement>? elements = _elements?.ToList();
                UnturnedUIProvider.Instance.DispatchToValidThread(() =>
                {
                    IntlDispose(elements, src2);
                });
            }
        }
        else
        {
            // ReSharper disable once InconsistentlySynchronizedField
            IntlDispose(_elements, src);
        }
    }

    /// <summary>
    /// This function is invoked after <see cref="Dispose()"/> is called before elements get cleaned up.
    /// </summary>
    /// <remarks>This will always be invoked on the game thread. The base method has no implementation and doesn't need to be invoked.</remarks>
    protected virtual void OnDisposing() { }

    private void IntlDispose(IReadOnlyList<UnturnedUIElement>? elements, IUnturnedUIDataSource src)
    {
        try
        {
            OnDisposing();
        }
        catch (Exception ex)
        {
            GetLogger().LogError(ex, string.Format(Properties.Resources.Log_ErrorDisposingUnturnedUI, Accessor.Formatter.Format(GetType())));
        }

        src.RemoveOwner(this);

        if (elements == null)
            return;

        lock (elements)
        {
            if (DebugLogging)
                GetLogger().LogInformation(Properties.Resources.Log_DeregisteringPrimitives, Name, elements.Count);

            for (int i = 0; i < elements.Count; ++i)
            {
                UnturnedUIElement element = elements[i];
                src.RemoveElement(element);
                element.DeregisterOwnerIntl();
                if (element is IDisposable disp)
                    disp.Dispose();
            }
        }
    }
    ~UnturnedUI()
    {
        Dispose(false);
    }
}