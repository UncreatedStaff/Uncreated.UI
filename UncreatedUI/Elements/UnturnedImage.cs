using DanielWillett.ReflectionTools;
using Microsoft.Extensions.Logging;
using SDG.NetTransport;
using SDG.Unturned;
using System;
using System.Diagnostics;
using Uncreated.Framework.UI.Presets;

namespace Uncreated.Framework.UI;

/// <summary>
/// Represents a web image in a Unity UI.
/// </summary>
[DebuggerDisplay("{ElementTypeDisplayName,nq} {Path}")]
public class UnturnedImage : UnturnedUIElement, IImage
{
    /// <summary>
    /// If the image should be cached on the client in the first place.
    /// </summary>
    public bool ShouldCache { get; set; } = true;

    /// <inheritdoc />
    protected override string ElementTypeDisplayName => Properties.Resources.DisplayName_UnturnedImage;

    public UnturnedImage(string path) : base(path) { }

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
        try
        {
            if (player.player.isActiveAndEnabled)
                SetImageIntl(player.transportConnection, url, forceRefresh);
        }
        catch (Exception ex)
        {
            GetLogger().LogWarning(ex, Properties.Resources.Log_ErrorPlayerOffline);
            GetLogger().LogDebug(new StackTrace().ToString());
        }
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
        try
        {
            if (player.isActiveAndEnabled)
                SetImageIntl(player.channel.owner.transportConnection, url, forceRefresh);
        }
        catch (Exception ex)
        {
            GetLogger().LogWarning(ex, Properties.Resources.Log_ErrorPlayerOffline);
            GetLogger().LogDebug(new StackTrace().ToString());
        }
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
        try
        {
            SetImageIntl(connection, url, forceRefresh);
        }
        catch (Exception ex)
        {
            GetLogger().LogWarning(ex, Properties.Resources.Log_ErrorPlayerOffline);
            GetLogger().LogDebug(new StackTrace().ToString());
        }
    }
    private void SetImageIntl(ITransportConnection connection, string url, bool forceRefresh)
    {
        AssertOwnerSet();

        UnturnedUIProvider.Instance.SetElementImageUrl(Owner.Key, connection, Owner.IsReliable, Path, url, ShouldCache, forceRefresh);
        if (Owner.DebugLogging)
        {
            GetLogger().LogInformation(Properties.Resources.Log_UnturnedImageUpdated, Owner.Name, Name, Owner.Key, url, forceRefresh);
        }
    }

    [Ignore]
    UnturnedImage IImage.Image => this;
}