using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SDG.NetTransport;
using SDG.Unturned;
using System;
using System.Globalization;
using System.Threading;
using Uncreated.Framework.UI.Data;
using Uncreated.Framework.UI.Presets;

namespace Uncreated.Framework.UI;

/// <summary>
/// Represents an input component in a Unity UI.
/// </summary>
public class UnturnedTextBox : UnturnedLabel, IDisposable, ITextBox
{
    private int _disposed;

    /// <summary>
    /// Called when text is committed by the user.
    /// </summary>
    public event TextUpdated? OnTextUpdated;

    /// <inheritdoc />
    protected override string ElementTypeDisplayName => "Text Box";

    /// <summary>
    /// Whether or not to save input to <see cref="UnturnedTextBoxData"/>.
    /// </summary>
    /// <remarks>Default: <see langword="false"/>.</remarks>
    public bool UseData { get; set; } = false;

    /// <exception cref="InvalidOperationException"><see cref="GlobalLogger.Instance"/> not initialized.</exception>
    public UnturnedTextBox(string path) : base(GlobalLogger.Instance, path) { }
    public UnturnedTextBox(ILogger logger, string path) : base(logger, path)
    {
        EffectManagerListener.RegisterTextBox(Name.ToString(), this);
    }
    public UnturnedTextBox(ILoggerFactory factory, string path) : base(factory, path)
    {
        EffectManagerListener.RegisterTextBox(Name.ToString(), this);
    }
    internal void InvokeOnTextCommitted(Player player, string text)
    {
        if (UseData)
        {
            UnturnedTextBoxData? data = UnturnedUIDataSource.GetData<UnturnedTextBoxData>(player.channel.owner.playerID.steamID, this);
            if (data == null)
                UnturnedUIDataSource.AddData(new UnturnedTextBoxData(player.channel.owner.playerID.steamID, this, text));
            else data.Text = text;
        }

        if (Owner.DebugLogging)
        {
            Logger.LogInformation("[{0}] [{1}] {{{2}}} Text committed by {3}, text: \"{4}\".", Owner.Name, Name, Owner.Key, player.channel.owner.playerID.steamID.m_SteamID.ToString(CultureInfo.InvariantCulture), text);
        }

        try
        {
            OnTextUpdated?.Invoke(this, player, text);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[{0}] [{1}] Error invoking {2}.", Owner.Name, Name, nameof(OnTextUpdated));
        }
    }

    /// <inheritdoc />
    public override void SetText(ITransportConnection connection, string? text)
    {
        text ??= string.Empty;
        if (!UseData)
        {
            base.SetText(connection, text);
            return;
        }

        if (Thread.CurrentThread.IsGameThread())
        {
            SetTextIntl(connection, text);
        }
        else
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            ITransportConnection c2 = connection;
            string txt2 = text;
            UniTask.Create(async () =>
            {
                await UniTask.SwitchToMainThread();
                SetTextIntl(c2, txt2);
            });
        }
    }

    /// <inheritdoc />
    public override void SetText(Player player, string? text)
    {
        text ??= string.Empty;
        if (!UseData)
        {
            base.SetText(player, text);
            return;
        }

        if (Thread.CurrentThread.IsGameThread())
        {
            SetTextIntl(player, text);
        }
        else
        {
            if (player is null)
                throw new ArgumentNullException(nameof(player));

            Player p2 = player;
            string txt2 = text;
            UniTask.Create(async () =>
            {
                await UniTask.SwitchToMainThread();
                SetTextIntl(p2, txt2);
            });
        }
    }

    /// <inheritdoc />
    public override void SetText(SteamPlayer player, string text)
    {
        text ??= string.Empty;
        if (!UseData)
        {
            base.SetText(player, text);
            return;
        }

        if (Thread.CurrentThread.IsGameThread())
        {
            SetTextIntl(player, text);
        }
        else
        {
            if (player is null)
                throw new ArgumentNullException(nameof(player));

            SteamPlayer p2 = player;
            string txt2 = text;
            UniTask.Create(async () =>
            {
                await UniTask.SwitchToMainThread();
                SetTextIntl(p2, txt2);
            });
        }
    }

    /// <summary>
    /// Returns data associated with this text box. Specify the default text in <paramref name="defaultText"/> if the data doesn't exist.
    /// </summary>
    public UnturnedTextBoxData GetOrAddData(Player player, string? defaultText = null)
    {
        UnturnedTextBoxData? data = UnturnedUIDataSource.GetData<UnturnedTextBoxData>(player.channel.owner.playerID.steamID, this);
        if (data == null)
            UnturnedUIDataSource.AddData(data = new UnturnedTextBoxData(player.channel.owner.playerID.steamID, this, defaultText));

        return data;
    }

    /// <summary>
    /// Gets the value from <see cref="UnturnedTextBoxData"/> and replicates it. Replicates and sets it to <paramref name="defaultValue"/> instead if data is not found.
    /// </summary>
    /// <remarks>Thread Safe</remarks>
    public void UpdateFromData(Player player, string defaultValue = "", bool callEvent = false)
    {
        if (Thread.CurrentThread.IsGameThread())
        {
            UpdateFromDataMainThread(player, defaultValue, callEvent);
        }
        else
        {
            if (player is null)
                throw new ArgumentNullException(nameof(player));

            Player p2 = player;
            string dv2 = defaultValue;
            bool call2 = callEvent;
            UniTask.Create(async () =>
            {
                await UniTask.SwitchToMainThread();
                UpdateFromDataMainThread(p2, dv2, call2);
            });
        }
    }

    /// <summary>
    /// Gets the value from <see cref="UnturnedTextBoxData"/> and replicates it. Replicates and sets it to <paramref name="defaultValue"/> instead if data is not found.
    /// </summary>
    /// <remarks>Must be ran on main thread.</remarks>
    /// <returns>The value of the input box after setting.</returns>
    /// <exception cref="NotSupportedException">Not ran on main thread.</exception>
    public string UpdateFromDataMainThread(Player player, string defaultValue = "", bool callEvent = false)
    {
        ThreadUtil.assertIsGameThread();

        UnturnedTextBoxData? messageData = UnturnedUIDataSource.GetData<UnturnedTextBoxData>(player.channel.owner.playerID.steamID, this);
        string val;
        if (messageData != null)
            base.SetText(player.channel.owner.transportConnection, val = messageData.Text ?? defaultValue);
        else
        {
            base.SetText(player.channel.owner.transportConnection, val = defaultValue);
            messageData = new UnturnedTextBoxData(player.channel.owner.playerID.steamID, Owner, this, defaultValue);
            UnturnedUIDataSource.AddData(messageData);
        }
        
        if (callEvent)
        {
            InvokeOnTextCommitted(player, val);
        }
        else if (Owner.DebugLogging)
        {
            Logger.LogInformation("[{0}] [{1}] {{{2}}} Text updated from data for {3}, text: \"{4}\" (event not invoked).", Owner.Name, Name, Owner.Key, player.channel.owner.playerID.steamID.m_SteamID.ToString(CultureInfo.InvariantCulture), val);
        }

        return val;
    }
    private void SetTextIntl(SteamPlayer player, string text)
    {
        base.SetText(player, text);

        if (player?.playerID is null)
            return;

        UnturnedTextBoxData? data = UnturnedUIDataSource.GetData<UnturnedTextBoxData>(player.playerID.steamID, this);
        if (data == null)
        {
            UnturnedUIDataSource.AddData(new UnturnedTextBoxData(player.playerID.steamID, this, text));
        }
        else
        {
            data.Text = text;
        }
    }
    private void SetTextIntl(Player player, string text)
    {
        base.SetText(player, text);

        if (player.channel?.owner?.playerID is null)
            return;

        UnturnedTextBoxData? data = UnturnedUIDataSource.GetData<UnturnedTextBoxData>(player.channel.owner.playerID.steamID, this);
        if (data == null)
        {
            UnturnedUIDataSource.AddData(new UnturnedTextBoxData(player.channel.owner.playerID.steamID, this, text));
        }
        else
        {
            data.Text = text;
        }
    }
    private void SetTextIntl(ITransportConnection connection, string text)
    {
        base.SetText(connection, text);
        if (Provider.clients == null)
            return;

        SteamPlayer? sp = null;
        for (int i = 0; i < Provider.clients.Count; ++i)
        {
            if (!Equals(Provider.clients[i].transportConnection, connection))
                continue;

            sp = Provider.clients[i];
            break;
        }

        if (sp == null)
            return;

        UnturnedTextBoxData? data = UnturnedUIDataSource.GetData<UnturnedTextBoxData>(sp.playerID.steamID, this);
        if (data == null)
        {
            UnturnedUIDataSource.AddData(new UnturnedTextBoxData(sp.playerID.steamID, this, text));
        }
        else
        {
            data.Text = text;
        }
    }

    /// <inheritdoc />
    void IDisposable.Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        EffectManagerListener.DeregisterTextBox(Name.ToString());
    }

    UnturnedTextBox ITextBox.TextBox => this;
}

/// <summary>
/// Handles text being updated in a <see cref="UnturnedTextBox"/>.
/// </summary>
public delegate void TextUpdated(UnturnedTextBox textBox, Player player, string text);