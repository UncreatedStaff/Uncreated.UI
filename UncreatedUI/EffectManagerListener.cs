using SDG.Unturned;
using System;
using System.Collections.Generic;

namespace Uncreated.Framework.UI;

internal static class EffectManagerListener
{
    private static bool _btnSubbed;
    private static bool _inpSubbed;

    private static readonly Dictionary<string, UnturnedButton> Buttons = new Dictionary<string, UnturnedButton>(StringComparer.Ordinal);
    private static readonly Dictionary<string, UnturnedTextBox> TextBoxes = new Dictionary<string, UnturnedTextBox>(StringComparer.Ordinal);
    
    internal static void DeregisterButton(string name)
    {
        lock (Buttons)
        {
            if (!Buttons.Remove(name))
                return;

            if (Buttons.Count != 0 || !_btnSubbed)
                return;

            UnturnedUIProvider.Instance.OnButtonClicked -= OnEffectButtonClicked;
            _btnSubbed = false;
        }
    }

    internal static void DeregisterTextBox(string name)
    {
        lock (TextBoxes)
        {
            if (!TextBoxes.Remove(name))
                return;

            if (TextBoxes.Count != 0 || !_inpSubbed)
                return;

            UnturnedUIProvider.Instance.OnTextCommitted -= OnEffectTextCommitted;
            _inpSubbed = false;
        }
    }

    internal static void RegisterButton(string name, UnturnedButton button)
    {
        lock (Buttons)
        {
            if (!_btnSubbed)
            {
                UnturnedUIProvider.Instance.OnButtonClicked += OnEffectButtonClicked;
                _btnSubbed = true;
            }

            if (Buttons.TryGetValue(name, out UnturnedButton? oldButton))
            {
                button.Duplicate = oldButton;
                Buttons[name] = button;
            }
            else
            {
                Buttons.Add(name, button);
            }
        }
    }

    internal static void RegisterTextBox(string name, UnturnedTextBox textBox)
    {
        lock (TextBoxes)
        {
            if (!_inpSubbed)
            {
                UnturnedUIProvider.Instance.OnTextCommitted += OnEffectTextCommitted;
                _inpSubbed = true;
            }

            if (TextBoxes.TryGetValue(name, out UnturnedTextBox? oldTextBox))
            {
                textBox.Duplicate = oldTextBox;
                TextBoxes[name] = textBox;
            }
            else
            {
                TextBoxes.Add(name, textBox);
            }
        }
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