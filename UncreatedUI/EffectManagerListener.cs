using SDG.Unturned;
using System;
using System.Collections.Generic;

namespace Uncreated.Framework.UI;

internal static class EffectManagerListener
{
    private static bool _btnSubbed;
    private static bool _inpSubbed;

    private static readonly Dictionary<string, UnturnedButton> Buttons = new Dictionary<string, UnturnedButton>(32, StringComparer.Ordinal);
    private static readonly Dictionary<string, UnturnedTextBox> TextBoxes = new Dictionary<string, UnturnedTextBox>(32, StringComparer.Ordinal);
    internal static void DeregisterButton(string name)
    {
        if (ThreadQueue.Queue == null)
        {
            DeregisterButtonIntl(name);
            return;
        }

        ThreadQueue.Queue.RunOnMainThread(() =>
        {
            DeregisterButtonIntl(name);
        });
    }
    internal static void DeregisterInputBox(string name)
    {
        if (ThreadQueue.Queue == null)
        {
            DeregisterInputIntl(name);
            return;
        }

        ThreadQueue.Queue.RunOnMainThread(() =>
        {
            DeregisterInputIntl(name);
        });
    }
    internal static void RegisterButton(string name, UnturnedButton button)
    {
        if (ThreadQueue.Queue == null)
        {
            RegisterButtonIntl(name, button);
            return;
        }

        ThreadQueue.Queue.RunOnMainThread(() =>
        {
            RegisterButtonIntl(name, button);
        });
    }
    internal static void RegisterInputBox(string name, UnturnedTextBox textBox)
    {
        if (ThreadQueue.Queue == null)
        {
            RegisterInputIntl(name, textBox);
            return;
        }

        ThreadQueue.Queue.RunOnMainThread(() =>
        {
            RegisterInputIntl(name, textBox);
        });
    }
    private static void DeregisterButtonIntl(string name)
    {
        if (!Buttons.Remove(name))
            return;
        if (Buttons.Count == 0 && _btnSubbed)
        {
            EffectManager.onEffectButtonClicked -= OnEffectButtonClicked;
            _btnSubbed = false;
        }
    }
    private static void DeregisterInputIntl(string name)
    {
        if (!TextBoxes.Remove(name))
            return;
        if (TextBoxes.Count == 0 && _inpSubbed)
        {
            EffectManager.onEffectTextCommitted -= OnEffectTextCommitted;
            _inpSubbed = false;
        }
    }
    private static void RegisterButtonIntl(string name, UnturnedButton button)
    {
        if (!_btnSubbed)
        {
            EffectManager.onEffectButtonClicked += OnEffectButtonClicked;
            _btnSubbed = true;
        }
        Buttons[name] = button;
    }
    private static void RegisterInputIntl(string name, UnturnedTextBox textBox)
    {
        if (!_inpSubbed)
        {
            EffectManager.onEffectTextCommitted += OnEffectTextCommitted;
            _inpSubbed = true;
        }
        TextBoxes[name] = textBox;
    }
    private static void OnEffectTextCommitted(Player player, string inputName, string input)
    {
        if (TextBoxes.TryGetValue(inputName, out UnturnedTextBox box))
            box.InvokeOnTextCommitted(player, input);
    }
    private static void OnEffectButtonClicked(Player player, string buttonName)
    {
        if (Buttons.TryGetValue(buttonName, out UnturnedButton btn))
            btn.InvokeOnClicked(player);
    }
}