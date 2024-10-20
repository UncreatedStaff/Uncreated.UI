using Microsoft.Extensions.Logging;
using SDG.Unturned;
using Steamworks;
using System;
using System.Security;
using Uncreated.Framework.UI;
using Uncreated.Framework.UI.Data;

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
        GlobalLogger.Instance = factory;

        ILogger<Program> logger = factory.CreateLogger<Program>();

        TestUI ui = new TestUI();

        CSteamID playerId = new CSteamID(1);
        
        if (UnturnedUIDataSource.GetData<TestData>(playerId, ui) is not { } data)
        {
            data = new TestData { Owner = ui, Player = playerId };
            UnturnedUIDataSource.AddData(data);
        }

        data = UnturnedUIDataSource.GetData<TestData>(playerId, ui);

        TestPopupUI popup = new TestPopupUI();

        logger.LogInformation("Done");
        Console.ReadLine();
    }

    public class TestData : IUnturnedUIData
    {
        public required CSteamID Player { get; init; }
        public required UnturnedUI Owner { get; init; }
        public UnturnedUIElement? Element => null;
    }
}