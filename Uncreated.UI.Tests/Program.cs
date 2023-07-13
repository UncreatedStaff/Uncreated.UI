using SDG.NetTransport;
using SDG.Unturned;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using Uncreated.Framework;
using Uncreated.Warfare.Moderation;
using Action = System.Action;

namespace Uncreated.UI.Tests;

internal class Program
{
    static void Main(string[] args)
    {
        ThreadUtil.setupGameThread();
        Environment.CurrentDirectory = @"C:\SteamCMD\steamapps\common\U3DS\";
        ThreadQueue.Queue = new TestThreadQueue();
        Test();
        while (true)
        {
            ((TestThreadQueue)ThreadQueue.Queue).Tick();

            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo info = Console.ReadKey(true);
                if (info.Key == ConsoleKey.Escape)
                    break;
            }

            Thread.Yield();
        }

        Console.WriteLine("Done again.");
        Console.ReadLine();
    }
    private static void Test()
    {
        Warfare.Data.RegisterInitialSyncs();
        using ModerationUI testUI = new ModerationUI();
        TestTransportConnection t = new TestTransportConnection();

        Thread.Sleep(1000);

        Console.WriteLine("Done.");
    }
}
internal class TestThreadQueue : IThreadQueue
{
    private readonly ConcurrentQueue<ThreadResult> queue = new ConcurrentQueue<ThreadResult>();
    public void Tick()
    {
        while (queue.TryDequeue(out ThreadResult result))
        {
            try
            {
                ThreadQueue.FulfillThreadTask(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
    public void RunOnMainThread(Action action)
    {
        ThreadResult task = new MainThreadTask(false, CancellationToken.None).GetAwaiter();
        task.OnCompleted(action);
        queue.Enqueue(task);
    }
    public void RunOnMainThread(ThreadResult awaiter)
    {
        queue.Enqueue(awaiter);
    }
    public bool IsMainThread => Thread.CurrentThread.IsGameThread();
}

internal class TestTransportConnection : ITransportConnection
{
    public bool Equals(ITransportConnection other) => ReferenceEquals(other, this);
    public bool TryGetIPv4Address(out uint address)
    {
        address = 0;
        return false;
    }

    public bool TryGetPort(out ushort port)
    {
        port = 0;
        return false;
    }

    public IPAddress GetAddress() => IPAddress.Loopback;
    public string GetAddressString(bool withPort) => "127.0.0.1" + (withPort ? ":0" : string.Empty);

    public void CloseConnection() { }

    public void Send(byte[] buffer, long size, ENetReliability reliability)
    {
        Console.WriteLine("send " + size + " bytes (" + reliability + ")");
    }
}