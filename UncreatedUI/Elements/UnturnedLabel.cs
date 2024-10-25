using Cysharp.Threading.Tasks;
using DanielWillett.ReflectionTools;
using Microsoft.Extensions.Logging;
using SDG.NetTransport;
using SDG.Unturned;
using System;
using System.Threading;
using Uncreated.Framework.UI.Presets;

namespace Uncreated.Framework.UI;
/// <summary>
/// Represents a text component in a Unity UI.
/// </summary>
public class UnturnedLabel : UnturnedUIElement, ILabel
{
    /// <inheritdoc />
    protected override string ElementTypeDisplayName => "Label";

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

        if (Thread.CurrentThread.IsGameThread())
        {
            EffectManager.sendUIEffectText(Owner.Key, connection, Owner.IsReliable, Path, text);
        }
        else
        {
            ITransportConnection c2 = connection;
            string txt2 = text;
            UniTask.Create(async () =>
            {
                await UniTask.SwitchToMainThread();
                EffectManager.sendUIEffectText(Owner.Key, c2, Owner.IsReliable, Path, txt2);
            });
        }

        if (Owner.DebugLogging)
        {
            GetLogger().LogInformation("[{0}] [{1}] {{{2}}} Set label text: \"{3}\".", Owner.Name, Name, Owner.Key, text);
        }
    }

    [Ignore]
    UnturnedLabel ILabel.Label => this;
}