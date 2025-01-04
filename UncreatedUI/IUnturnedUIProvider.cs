using System;
using SDG.NetTransport;
using SDG.Unturned;

namespace Uncreated.Framework.UI;

/// <summary>
/// Thread-safe abstraction surrounding the vanilla UI API, used for unit tests.
/// </summary>
public interface IUnturnedUIProvider
{
    /// <summary>
    /// Invoked when a button is pressed by a player. Only contains the name of the element, not the full path.
    /// </summary>
    event EffectManager.EffectButtonClickedHandler OnButtonClicked;

    /// <summary>
    /// Invoked when text is committed to a text box by a player. Only contains the name of the element, not the full path.
    /// </summary>
    event EffectManager.EffectTextCommittedHandler OnTextCommitted;

    /// <summary>
    /// Checks if assets are still loading and <see cref="Level.onPrePreLevelLoaded"/> should load the asset.
    /// </summary>
    bool AreAssetsStillLoading { get; }

    /// <summary>
    /// If this thread is the optimized thread for calling APIs in this interface. Usually this is the 'game thread'.
    /// </summary>
    bool IsValidThread();

    /// <summary>
    /// Execute an action in fire-and-forget fashion on the valid thread.
    /// </summary>
    /// <remarks>Unexceptional.</remarks>
    void DispatchToValidThread(System.Action action);

    /// <summary>
    /// Clear an effect by it's ID for a single player.
    /// </summary>
    void ClearById(ushort id, ITransportConnection connection);

    /// <summary>
    /// Clear an effect by it's ID for all players.
    /// </summary>
    void ClearByIdGlobal(ushort id);

    /// <summary>
    /// Sets the visibility of an element on an effect for one player.
    /// </summary>
    /// <param name="key">A key unique to a given element.</param>
    /// <param name="connection">The transport connection of the player to set the effect for.</param>
    /// <param name="isReliable">If it should be set over the reliable buffer, guaranteeing that it makes it to the player.</param>
    /// <param name="path">Full path to the element.</param>
    /// <param name="state">Visibility state. <see langword="true"/> is visible, <see langword="false"/> is invisible.</param>
    void SetElementVisibility(short key, ITransportConnection connection, bool isReliable, string path, bool state);

    /// <summary>
    /// Sets the text of a label element on an effect for one player.
    /// </summary>
    /// <param name="key">A key unique to a given element.</param>
    /// <param name="connection">The transport connection of the player to set the effect for.</param>
    /// <param name="isReliable">If it should be set over the reliable buffer, guaranteeing that it makes it to the player.</param>
    /// <param name="path">Full path to the element.</param>
    /// <param name="text">The text to set for the label.</param>
    void SetElementText(short key, ITransportConnection connection, bool isReliable, string path, string text);

    /// <summary>
    /// Sets the URL of an image element on an effect for one player.
    /// </summary>
    /// <param name="key">A key unique to a given element.</param>
    /// <param name="connection">The transport connection of the player to set the effect for.</param>
    /// <param name="isReliable">If it should be set over the reliable buffer, guaranteeing that it makes it to the player.</param>
    /// <param name="path">Full path to the element.</param>
    /// <param name="imageUrl">Full HTTP or HTTPS URL for the image on the internet.</param>
    /// <param name="shouldCache">If the image can be cached by the player.</param>
    /// <param name="forceRefresh">If the sending image should ignore the cached image and re-request if it's cached.</param>
    void SetElementImageUrl(short key, ITransportConnection connection, bool isReliable, string path, string imageUrl, bool shouldCache, bool forceRefresh);

    /// <summary>
    /// Send a new UI effect to a single player.
    /// </summary>
    /// <param name="id">The ID of the effect.</param>
    /// <param name="key">A unique key used to reference the effect later on. Must not be -1.</param>
    /// <param name="connection">The transport connection of the player to send the UI to.</param>
    /// <param name="isReliable">If it should be set over the reliable buffer, guaranteeing that it makes it to the player.</param>
    void SendUI(ushort id, short key, ITransportConnection connection, bool isReliable);

    /// <summary>
    /// Send a new UI effect to a single player.
    /// </summary>
    /// <param name="id">The ID of the effect.</param>
    /// <param name="key">A unique key used to reference the effect later on. Must not be -1.</param>
    /// <param name="connection">The transport connection of the player to send the UI to.</param>
    /// <param name="isReliable">If it should be set over the reliable buffer, guaranteeing that it makes it to the player.</param>
    /// <param name="arg0">Text to replace {0} in all text elements.</param>
    void SendUI(ushort id, short key, ITransportConnection connection, bool isReliable, string arg0);

    /// <summary>
    /// Send a new UI effect to a single player.
    /// </summary>
    /// <param name="id">The ID of the effect.</param>
    /// <param name="key">A unique key used to reference the effect later on. Must not be -1.</param>
    /// <param name="connection">The transport connection of the player to send the UI to.</param>
    /// <param name="isReliable">If it should be set over the reliable buffer, guaranteeing that it makes it to the player.</param>
    /// <param name="arg0">Text to replace {0} in all text elements.</param>
    /// <param name="arg1">Text to replace {1} in all text elements.</param>
    void SendUI(ushort id, short key, ITransportConnection connection, bool isReliable, string arg0, string arg1);

    /// <summary>
    /// Send a new UI effect to a single player.
    /// </summary>
    /// <param name="id">The ID of the effect.</param>
    /// <param name="key">A unique key used to reference the effect later on. Must not be -1.</param>
    /// <param name="connection">The transport connection of the player to send the UI to.</param>
    /// <param name="isReliable">If it should be set over the reliable buffer, guaranteeing that it makes it to the player.</param>
    /// <param name="arg0">Text to replace {0} in all text elements.</param>
    /// <param name="arg1">Text to replace {1} in all text elements.</param>
    /// <param name="arg2">Text to replace {2} in all text elements.</param>
    void SendUI(ushort id, short key, ITransportConnection connection, bool isReliable, string arg0, string arg1, string arg2);

    /// <summary>
    /// Send a new UI effect to a single player.
    /// </summary>
    /// <param name="id">The ID of the effect.</param>
    /// <param name="key">A unique key used to reference the effect later on. Must not be -1.</param>
    /// <param name="connection">The transport connection of the player to send the UI to.</param>
    /// <param name="isReliable">If it should be set over the reliable buffer, guaranteeing that it makes it to the player.</param>
    /// <param name="arg0">Text to replace {0} in all text elements.</param>
    /// <param name="arg1">Text to replace {1} in all text elements.</param>
    /// <param name="arg2">Text to replace {2} in all text elements.</param>
    /// <param name="arg3">Text to replace {3} in all text elements.</param>
    void SendUI(ushort id, short key, ITransportConnection connection, bool isReliable, string arg0, string arg1, string arg2, string arg3);

    /// <summary>
    /// Send a new UI effect to all players.
    /// </summary>
    /// <param name="id">The ID of the effect.</param>
    /// <param name="key">A unique key used to reference the effect later on. Must not be -1.</param>
    /// <param name="isReliable">If it should be set over the reliable buffer, guaranteeing that it makes it to the players.</param>
    void SendUIGlobal(ushort id, short key, bool isReliable);

    /// <summary>
    /// Send a new UI effect to all players.
    /// </summary>
    /// <param name="id">The ID of the effect.</param>
    /// <param name="key">A unique key used to reference the effect later on. Must not be -1.</param>
    /// <param name="isReliable">If it should be set over the reliable buffer, guaranteeing that it makes it to the players.</param>
    /// <param name="arg0">Text to replace {0} in all text elements.</param>
    void SendUIGlobal(ushort id, short key, bool isReliable, string arg0);

    /// <summary>
    /// Send a new UI effect to all players.
    /// </summary>
    /// <param name="id">The ID of the effect.</param>
    /// <param name="key">A unique key used to reference the effect later on. Must not be -1.</param>
    /// <param name="isReliable">If it should be set over the reliable buffer, guaranteeing that it makes it to the players.</param>
    /// <param name="arg0">Text to replace {0} in all text elements.</param>
    /// <param name="arg1">Text to replace {1} in all text elements.</param>
    void SendUIGlobal(ushort id, short key, bool isReliable, string arg0, string arg1);

    /// <summary>
    /// Send a new UI effect to all players.
    /// </summary>
    /// <param name="id">The ID of the effect.</param>
    /// <param name="key">A unique key used to reference the effect later on. Must not be -1.</param>
    /// <param name="isReliable">If it should be set over the reliable buffer, guaranteeing that it makes it to the players.</param>
    /// <param name="arg0">Text to replace {0} in all text elements.</param>
    /// <param name="arg1">Text to replace {1} in all text elements.</param>
    /// <param name="arg2">Text to replace {2} in all text elements.</param>
    void SendUIGlobal(ushort id, short key, bool isReliable, string arg0, string arg1, string arg2);

    /// <summary>
    /// Send a new UI effect to all players.
    /// </summary>
    /// <param name="id">The ID of the effect.</param>
    /// <param name="key">A unique key used to reference the effect later on. Must not be -1.</param>
    /// <param name="isReliable">If it should be set over the reliable buffer, guaranteeing that it makes it to the players.</param>
    /// <param name="arg0">Text to replace {0} in all text elements.</param>
    /// <param name="arg1">Text to replace {1} in all text elements.</param>
    /// <param name="arg2">Text to replace {2} in all text elements.</param>
    /// <param name="arg3">Text to replace {3} in all text elements.</param>
    void SendUIGlobal(ushort id, short key, bool isReliable, string arg0, string arg1, string arg2, string arg3);

    /// <summary>
    /// Finds the <see cref="EffectAsset"/> for a given short ID.
    /// </summary>
    EffectAsset? GetEffectAsset(ushort id);

    /// <summary>
    /// Finds the <see cref="EffectAsset"/> for a given GUID.
    /// </summary>
    EffectAsset? GetEffectAsset(Guid guid);
}