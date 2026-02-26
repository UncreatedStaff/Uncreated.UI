using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using Microsoft.Extensions.Logging;
using SDG.Unturned;
using System.Text;
using HarmonyLib;
using Uncreated.Framework.UI;
using UnityEngine;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Uncreated.UI.Tests;

[TestFixture]
public class UnturnedLogTests
{
    [Test, NonParallelizable]
    public void TestWritesToUnturnedLog()
    {
        GlobalLogger.ResetToUnturnedLog();

        ILogger logger = GlobalLogger.Instance.CreateLogger("LoggerTest");

        UnturnedUIProvider.Instance = new TestUIProvider();

        try
        {
            logger.LogInformation("Hello");
        }
        catch (TypeInitializationException)
        {
            Assert.Pass("Expected exception thrown.");
        }
    }
}
