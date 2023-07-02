using System.Collections.Generic;

namespace Uncreated.Framework.UI;

public class ElementCollection : List<UnturnedUIElement>
{
    public UnturnedUI Owner { get; }
    public ElementCollection(UnturnedUI owner, int capacity = 16) : base(capacity)
    {
        Owner = owner;
    }
}