using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Steamworks;
using Uncreated.Framework.UI;
using Uncreated.Framework.UI.Data;
using Uncreated.Framework.UI.Patterns;
using Uncreated.Framework.UI.Reflection;

namespace Uncreated.UI.Tests;

public class Sandbox
{
    [Test]
    public void SandboxTest()
    {
        SomeUI ui = new SomeUI();
        Console.WriteLine("hello");
    }
    // put your UI here

    public class SomeUI() : UnturnedUI(Guid.Empty)
    {

    }
}
