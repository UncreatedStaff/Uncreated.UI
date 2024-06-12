using Cysharp.Threading.Tasks;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Uncreated.Framework.UI;

internal static class EffectManagerListener
{
    private static bool _btnSubbed;
    private static bool _inpSubbed;

    private static readonly Dictionary<string, UnturnedButton> Buttons = new Dictionary<string, UnturnedButton>(32, StringComparer.Ordinal);
    private static readonly Dictionary<string, UnturnedTextBox> TextBoxes = new Dictionary<string, UnturnedTextBox>(32, StringComparer.Ordinal);
    internal static void DeregisterButton(string name)
    {
        if (Thread.CurrentThread.IsGameThread())
        {
            DeregisterButtonIntl(name);
        }
        else
        {
            string n2 = name;
            UniTask.Create(async () =>
            {
                await UniTask.SwitchToMainThread();
                DeregisterButtonIntl(n2);
            });
        }
    }
    internal static void DeregisterTextBox(string name)
    {
        if (Thread.CurrentThread.IsGameThread())
        {
            DeregisterInputIntl(name);
        }
        else
        {
            string n2 = name;
            UniTask.Create(async () =>
            {
                await UniTask.SwitchToMainThread();
                DeregisterInputIntl(n2);
            });
        }
    }
    internal static void RegisterButton(string name, UnturnedButton button)
    {
        if (Thread.CurrentThread.IsGameThread())
        {
            RegisterButtonIntl(name, button);
        }
        else
        {
            string n2 = name;
            UnturnedButton b2 = button;
            UniTask.Create(async () =>
            {
                await UniTask.SwitchToMainThread();
                RegisterButtonIntl(n2, b2);
            });
        }
    }
    internal static void RegisterTextBox(string name, UnturnedTextBox textBox)
    {
        if (Thread.CurrentThread.IsGameThread())
        {
            RegisterInputIntl(name, textBox);
        }
        else
        {
            string n2 = name;
            UnturnedTextBox b2 = textBox;
            UniTask.Create(async () =>
            {
                await UniTask.SwitchToMainThread();
                RegisterInputIntl(n2, b2);
            });
        }
    }
    private static void DeregisterButtonIntl(string name)
    {
        if (!Buttons.Remove(name))
            return;

        if (Buttons.Count != 0 || !_btnSubbed)
            return;

        EffectManager.onEffectButtonClicked -= OnEffectButtonClicked;
        _btnSubbed = false;
    }
    private static void DeregisterInputIntl(string name)
    {
        if (!TextBoxes.Remove(name))
            return;

        if (TextBoxes.Count != 0 || !_inpSubbed)
            return;

        EffectManager.onEffectTextCommitted -= OnEffectTextCommitted;
        _inpSubbed = false;
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