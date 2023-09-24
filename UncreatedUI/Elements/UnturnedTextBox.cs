using SDG.Unturned;
using System;
using System.Globalization;
using SDG.NetTransport;
using Uncreated.Framework.UI.Data;
using Uncreated.Networking;

namespace Uncreated.Framework.UI;
public delegate void TextUpdated(UnturnedTextBox textBox, Player player, string text);
/// <summary>
/// Represents an input component in a Unity UI.
/// </summary>
public class UnturnedTextBox : UnturnedLabel, IDisposable
{
    private bool _disposed;
    /// <summary>
    /// Called when text is committed by the user.
    /// </summary>
    public event TextUpdated? OnTextUpdated;

    /// <summary>
    /// Whether or not to save input to <see cref="UnturnedTextBoxData"/> or not.
    /// </summary>
    /// <remarks>Default: <see langword="false"/>.</remarks>
    public bool UseData { get; set; } = false;
    public UnturnedTextBox(string name) : base(name)
    {
        EffectManagerListener.RegisterInputBox(Name, this);
    }
    internal UnturnedTextBox(UnturnedTextBox original) : base(original)
    {
        EffectManagerListener.RegisterInputBox(Name, this);
    }
    ~UnturnedTextBox()
    {
        if (_disposed) return;
        Dispose(false);
    }
    private void Dispose(bool disposing)
    {
        _disposed = true;
        EffectManagerListener.DeregisterInputBox(Name);

        if (disposing)
            GC.SuppressFinalize(this);
    }
    public void Dispose()
    {
        if (_disposed) return;
        Dispose(true);
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
            Logging.LogInfo($"[{Owner.Name.ToUpperInvariant()}] [{Name.ToUpperInvariant()}] {{{Owner.Key}}} Text committed by {player.channel.owner.playerID.steamID.m_SteamID.ToString(CultureInfo.InvariantCulture)}, text: {text}.");
        }
        OnTextUpdated?.Invoke(this, player, text);
    }

    public override void SetText(ITransportConnection connection, string? text)
    {
        text ??= string.Empty;
        base.SetText(connection, text);

        if (UseData)
        {
            if (Dedicator.commandWindow == null || Provider.clients == null)
                return;
            SteamPlayer? sp = null;
            for (int i = 0; i < Provider.clients.Count; ++i)
            {
                if (Equals(Provider.clients[i].transportConnection, connection))
                {
                    sp = Provider.clients[i];
                    break;
                }
            }

            if (sp == null)
                return;

            UnturnedTextBoxData? data = UnturnedUIDataSource.GetData<UnturnedTextBoxData>(sp.playerID.steamID, this);
            if (data == null)
                UnturnedUIDataSource.AddData(new UnturnedTextBoxData(sp.playerID.steamID, this, text));
            else data.Text = text;
        }
    }
    public override void SetText(Player player, string? text)
    {
        text ??= string.Empty;
        base.SetText(player, text);

        if (UseData)
        {
            if (Dedicator.commandWindow == null || player.channel?.owner == null)
                return;

            UnturnedTextBoxData? data = UnturnedUIDataSource.GetData<UnturnedTextBoxData>(player.channel.owner.playerID.steamID, this);
            if (data == null)
                UnturnedUIDataSource.AddData(new UnturnedTextBoxData(player.channel.owner.playerID.steamID, this, text));
            else data.Text = text;
        }
    }
    public override void SetText(SteamPlayer player, string text)
    {
        text ??= string.Empty;
        base.SetText(player, text);

        if (UseData)
        {
            if (Dedicator.commandWindow == null)
                return;

            UnturnedTextBoxData? data = UnturnedUIDataSource.GetData<UnturnedTextBoxData>(player.playerID.steamID, this);
            if (data == null)
                UnturnedUIDataSource.AddData(new UnturnedTextBoxData(player.playerID.steamID, this, text));
            else data.Text = text;
        }
    }

    public override object Clone()
    {
        return new UnturnedTextBox(this);
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
        if (ThreadQueue.Queue.IsMainThread)
            UpdateFromDataMainThread(player, defaultValue, callEvent);
        else
            ThreadQueue.Queue.RunOnMainThread(() => UpdateFromDataMainThread(player, defaultValue, callEvent));
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
            InvokeOnTextCommitted(player, val);
        else if (Owner.DebugLogging)
            Logging.LogInfo($"[{Owner.Name.ToUpperInvariant()}] [{Name.ToUpperInvariant()}] {{{Owner.Key}}} Text updated from data for {player.channel.owner.playerID.steamID.m_SteamID.ToString(CultureInfo.InvariantCulture)}, text: {val} (event not invoked).");

        return val;
    }
}