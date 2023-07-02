using SDG.NetTransport;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Uncreated.Framework.UI;
public class UnturnedUIElement : ICloneable
{
    protected readonly string _name;
    protected UnturnedUI? _owner;
    protected UnturnedUIElement? _parent;
    public string Name => _name;

    /// <exception cref="InvalidOperationException">Thrown when the owner has yet to be set.</exception>
    public UnturnedUI Owner
    {
        get
        {
            AssertOwnerSet();
            return _owner!;
        }
    }
    public UnturnedUIElement? Parent { get; private set; }
    public UnturnedUIElement(string name)
    {
        _name = name;
    }
    protected UnturnedUIElement(UnturnedUIElement original)
    {
        _name = original._name;
    }
    protected void AssertOwnerSet()
    {
        if (_owner is null)
            throw new InvalidOperationException("UI owner has not yet been set. Make sure the element is a field in it's owner's class.");
    }
    internal void RegisterOwnerInternal(UnturnedUI? owner) => RegisterOwner(owner);
    internal void AddIncludedElementsInternal(List<UnturnedUIElement> elements)
    {
        int ct = elements.Count;
        AddIncludedElements(elements);
        for (int i = ct; i < elements.Count; ++i)
            elements[i].Parent = this;
    }
    protected virtual void RegisterOwner(UnturnedUI? owner)
    {
        this._owner = owner;
    }
    public void SetVisibility(ITransportConnection connection, bool isEnabled)
    {
        AssertOwnerSet();
        EffectManager.sendUIEffectVisibility(_owner!.Key, connection, _owner.IsReliable, _name, isEnabled);
    }
    protected virtual void AddIncludedElements(List<UnturnedUIElement> elements) { }
    public virtual object Clone()
    {
        return new UnturnedUIElement(this);
    }
    public static UnturnedUIElement[] GetPattern(string name, int length, int start = 1)
    {
        UnturnedUIElement[] elems = new UnturnedUIElement[length];
        for (int i = 0; i < length; ++i)
        {
            elems[i] = new UnturnedUIElement(Util.QuickFormat(name, (i + start).ToString(CultureInfo.InvariantCulture)));
        }

        return elems;
    }
}