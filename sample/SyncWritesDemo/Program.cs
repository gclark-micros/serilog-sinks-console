﻿using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SyncWritesDemo
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("A sample of how to sync writes to the console sink.");

            if (args != null && args.Length == 1)
            {
                switch (args[0])
                {
                    case "--sync-root-default":
                        SystemConsoleSyncTest(syncRootForLogger1: null, syncRootForLogger2: null);
                        return;
                    case "--sync-root-separate":
                        SystemConsoleSyncTest(syncRootForLogger1: new object(), syncRootForLogger2: new object());
                        return;
                    case "--sync-root-same":
                        var sameSyncRoot = new object();
                        SystemConsoleSyncTest(syncRootForLogger1: sameSyncRoot, syncRootForLogger2: sameSyncRoot);
                        return;
                }
            }

            Console.WriteLine("Expecting one of the following arguments:{0}--sync-root-default{0}--sync-root-separate{0}--sync-root-same", Environment.NewLine);
        }

        static void SystemConsoleSyncTest(object syncRootForLogger1, object syncRootForLogger2)
        {
            var logger1 = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.WithProperty("Logger", "logger1")
                .WriteTo.Console(theme: SystemConsoleTheme.Literate, syncRoot: syncRootForLogger1)
                .CreateLogger();

            var logger2 = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.WithProperty("Logger", "logger2")
                .WriteTo.Console(theme: SystemConsoleTheme.Literate, syncRoot: syncRootForLogger2)
                .CreateLogger();

            var options = new ParallelOptions { MaxDegreeOfParallelism = 8 };
            System.Threading.Tasks.Parallel.For(0, 1000, options, (i, loopState) =>
            {
                var logger = (i % 2 == 0) ? logger1 : logger2;
                logger.Information("Event {Iteration} generated by {ThreadId}", i, Thread.CurrentThread.ManagedThreadId);
            });
        }
    }
}