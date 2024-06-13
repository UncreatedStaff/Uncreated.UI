using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SDG.NetTransport;
using SDG.Unturned;
using System;
using System.Threading;
using Uncreated.Framework.UI.Presets;

namespace Uncreated.Framework.UI;

/// <summary>
/// Represents a web image in a Unity UI.
/// </summary>
public class UnturnedImage : UnturnedUIElement, IImage
{
    /// <summary>
    /// If the image should be cached on the client in the first place.
    /// </summary>
    public bool ShouldCache { get; set; } = true;

    /// <exception cref="InvalidOperationException"><see cref="GlobalLogger.Instance"/> not initialized.</exception>
    public UnturnedImage(string path) : this(GlobalLogger.Instance, path) { }
    public UnturnedImage(ILogger logger, string path) : base(logger, path) { }
    public UnturnedImage(ILoggerFactory factory, string path) : base(factory, path) { }

    /// <summary>
    /// Set the URL to the image displayed.
    /// </summary>
    /// <param name="player">Player to send the image to.</param>
    /// <param name="url">Direct URL to the image.</param>
    /// <param name="forceRefresh">Force the recipient to redownload the image even if it's already cached.</param>
    /// <exception cref="ArgumentNullException"/>
    public void SetImage(SteamPlayer player, string? url, bool forceRefresh = false)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));

        url ??= string.Empty;
        if (player.player.isActiveAndEnabled)
            SetImageIntl(player.transportConnection, url, forceRefresh);
    }

    /// <summary>
    /// Set the URL to the image displayed.
    /// </summary>
    /// <param name="player">Player to send the image to.</param>
    /// <param name="url">Direct URL to the image.</param>
    /// <param name="forceRefresh">Force the recipient to redownload the image even if it's already cached.</param>
    /// <exception cref="ArgumentNullException"/>
    public virtual void SetImage(Player player, string? url, bool forceRefresh = false)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));

        url ??= string.Empty;
        if (player.isActiveAndEnabled)
            SetImageIntl(player.channel.owner.transportConnection, url, forceRefresh);
    }

    /// <summary>
    /// Set the URL to the image displayed.
    /// </summary>
    /// <param name="connection">Connection to send the image to.</param>
    /// <param name="url">Direct URL to the image.</param>
    /// <param name="forceRefresh">Force the recipient to redownload the image even if it's already cached.</param>
    /// <exception cref="ArgumentNullException"/>
    public virtual void SetImage(ITransportConnection connection, string? url, bool forceRefresh = false)
    {
        if (connection == null)
            throw new ArgumentNullException(nameof(connection));

        url ??= string.Empty;
        SetImageIntl(connection, url, forceRefresh);
    }
    private void SetImageIntl(ITransportConnection connection, string url, bool forceRefresh)
    {
        AssertOwnerSet();

        if (Thread.CurrentThread.IsGameThread())
        {
            EffectManager.sendUIEffectImageURL(Owner.Key, connection, Owner.IsReliable, Path, url, ShouldCache, forceRefresh);
        }
        else
        {
            ITransportConnection c2 = connection;
            string url2 = url;
            bool fr2 = forceRefresh;
            UniTask.Create(async () =>
            {
                await UniTask.SwitchToMainThread();
                EffectManager.sendUIEffectImageURL(Owner.Key, c2, Owner.IsReliable, Path, url2, ShouldCache, fr2);
            });
        }

        if (Owner.DebugLogging)
        {
            Logger.LogInformation("[{0}] [{1}] {{{2}}} Set image URL, link: {3}, force refresh: {4}.", Owner.Name, Name, Owner.Key, url, forceRefresh);
        }
    }

    UnturnedImage IImage.Image => this;
}