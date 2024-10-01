using Microsoft.Extensions.Logging;
using SDG.Unturned;
using System;
using System.Security;
using Uncreated.Framework.UI;
using Uncreated.Framework.UI.Patterns;

namespace Uncreated.UI.Tests;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Test format: " + UnturnedUIUtility.QuickFormat("{0}_a", "value", 0, '_'));
        Console.WriteLine("Test format: " + UnturnedUIUtility.QuickFormat("{0}", "value", 0, '_'));
        Console.WriteLine("Test format: " + UnturnedUIUtility.QuickFormat("_{0}", "value", 0, '_'));
        Console.WriteLine("Test format: " + UnturnedUIUtility.QuickFormat("a_{0}", "value", 0, '_'));
        Console.WriteLine("Test format: " + UnturnedUIUtility.QuickFormat("{0}_", "value", 0, '_'));
        Console.WriteLine("Test format: " + UnturnedUIUtility.QuickFormat("_{0}_a", string.Empty, 0, '_'));

        try
        {
            ThreadUtil.setupGameThread();
        }
        catch (SecurityException)
        {
            // ignored
        }

        ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
        GlobalLogger.Instance = factory.CreateLogger<Program>();

        TestUI ui = new TestUI();

        GlobalLogger.Instance.LogInformation("Done");
        Console.ReadLine();
    }
}