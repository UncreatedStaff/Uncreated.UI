using SDG.Unturned;
using System;
using System.Globalization;

namespace Uncreated.Framework.UI;
public delegate void ButtonClicked(UnturnedButton button, Player player);
public class UnturnedButton : UnturnedUIElement, IDisposable
{
    private bool _disposed;
    public event ButtonClicked? OnClicked;
    public UnturnedButton(string name) : base(name)
    {
        EffectManagerListener.RegisterButton(name, this);
    }
    internal UnturnedButton(UnturnedButton original) : base(original)
    {
        EffectManagerListener.RegisterButton(original.Name, this);
    }
    ~UnturnedButton()
    {
        if (_disposed) return;
        Dispose(false);
    }
    private void Dispose(bool disposing)
    {
        _disposed = true;
        EffectManagerListener.DeregisterButton(Name);

        if (disposing)
            GC.SuppressFinalize(this);
    }
    public void Dispose()
    {
        if (_disposed) return;
        Dispose(true);
    }

    internal void InvokeOnClicked(Player player) => OnClicked?.Invoke(this, player);
    public override object Clone()
    {
        return new UnturnedButton(this);
    }
    public new static UnturnedButton[] GetPattern(string name, int length, int start = 1)
    {
        UnturnedButton[] elems = new UnturnedButton[length];
        for (int i = 0; i < length; ++i)
        {
            elems[i] = new UnturnedButton(Util.QuickFormat(name, (i + start).ToString(CultureInfo.InvariantCulture)));
        }

        return elems;
    }
}