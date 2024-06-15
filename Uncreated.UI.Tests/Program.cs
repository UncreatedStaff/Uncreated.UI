using Microsoft.Extensions.Logging;
using SDG.Unturned;
using System;
using System.Security;
using Uncreated.Framework.UI;

namespace Uncreated.UI.Tests;

internal class Program
{
    static void Main(string[] args)
    {
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