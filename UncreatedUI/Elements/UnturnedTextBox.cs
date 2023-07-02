using SDG.Unturned;
using System;
using System.Globalization;

namespace Uncreated.Framework.UI;
public delegate void TextUpdated(UnturnedTextBox button, Player player, string text);
public class UnturnedTextBox : UnturnedUIElement
{
    private bool _disposed;
    public event TextUpdated? OnTextUpdated;
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
    internal void InvokeOnTextCommitted(Player player, string text) => OnTextUpdated?.Invoke(this, player, text);
    public override object Clone()
    {
        return new UnturnedTextBox(this);
    }
    public new static UnturnedTextBox[] GetPattern(string name, int length, int start = 1)
    {
        UnturnedTextBox[] elems = new UnturnedTextBox[length];
        for (int i = 0; i < length; ++i)
        {
            elems[i] = new UnturnedTextBox(Util.QuickFormat(name, (i + start).ToString(CultureInfo.InvariantCulture)));
        }

        return elems;
    }
}