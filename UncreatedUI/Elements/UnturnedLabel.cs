using DanielWillett.ReflectionTools;
using Microsoft.Extensions.Logging;
using SDG.NetTransport;
using SDG.Unturned;
using System;
using Uncreated.Framework.UI.Presets;

namespace Uncreated.Framework.UI;
/// <summary>
/// Represents a text component in a Unity UI.
/// </summary>
public class UnturnedLabel : UnturnedUIElement, ILabel
{
    /// <inheritdoc />
    protected override string ElementTypeDisplayName => Properties.Resources.DisplayName_UnturnedLabel;

    public UnturnedLabel(string path) : base(path) { }

    /// <summary>
    /// Set the text to be displayed on this label.
    /// </summary>
    /// <param name="player">Player to send the image to.</param>
    /// <param name="text">Text to set on the label.</param>
    /// <exception cref="ArgumentNullException"/>
    public virtual void SetText(SteamPlayer player, string text)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));

        text ??= string.Empty;
        if (player.player.isActiveAndEnabled)
            SetTextIntl(player.transportConnection, text);
    }

    /// <summary>
    /// Set the text to be displayed on this label.
    /// </summary>
    /// <param name="player">Player to send the image to.</param>
    /// <param name="text">Text to set on the label.</param>
    /// <exception cref="ArgumentNullException"/>
    public virtual void SetText(Player player, string? text)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));

        text ??= string.Empty;
        if (player.isActiveAndEnabled)
            SetTextIntl(player.channel.owner.transportConnection, text);
    }

    /// <summary>
    /// Set the text to be displayed on this label.
    /// </summary>
    /// <param name="connection">Connection to send the image to.</param>
    /// <param name="text">Text to set on the label.</param>
    /// <exception cref="ArgumentNullException"/>
    public virtual void SetText(ITransportConnection connection, string text)
    {
        if (connection == null)
            throw new ArgumentNullException(nameof(connection));

        text ??= string.Empty;
        SetTextIntl(connection, text);
    }
    private void SetTextIntl(ITransportConnection connection, string text)
    {
        AssertOwnerSet();

        UnturnedUIProvider.Instance.SetElementText(Owner.Key, connection, Owner.IsReliable, Path, text);

        if (Owner.DebugLogging)
        {
            GetLogger().LogInformation(Properties.Resources.Log_UnturnedLabelUpdated, Owner.Name, Name, Owner.Key, text);
        }
    }

    [Ignore]
    UnturnedLabel ILabel.Label => this;
}