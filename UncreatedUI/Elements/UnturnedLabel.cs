using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SDG.NetTransport;
using SDG.Unturned;
using System;
using System.Threading;

namespace Uncreated.Framework.UI;
/// <summary>
/// Represents a text component in a Unity UI.
/// </summary>
public class UnturnedLabel : UnturnedUIElement
{
    /// <exception cref="InvalidOperationException"><see cref="GlobalLogger.Instance"/> not initialized.</exception>
    public UnturnedLabel(string name) : this(GlobalLogger.Instance, name) { }
    public UnturnedLabel(ILogger logger, string name) : base(logger, name) { }
    public UnturnedLabel(ILoggerFactory factory, string name) : base(factory, name) { }
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
            EffectManager.sendUIEffectText(_owner!.Key, connection, _owner.IsReliable, Name, text);
        }
        else
        {
            ITransportConnection c2 = connection;
            string txt2 = text;
            UniTask.Create(async () =>
            {
                await UniTask.SwitchToMainThread();
                EffectManager.sendUIEffectText(_owner!.Key, c2, _owner.IsReliable, Name, txt2);
            });
        }

        if (Owner.DebugLogging)
        {
            Logger.LogInformation("[{0}] [{1}] {{{2}}} Set label text: \"{3}\".", Owner.Name, Name, Owner.Key, text);
        }
    }
}