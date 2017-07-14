﻿using System;
using TitanBot;
using TitanBot.Logging;

namespace LiveBaseTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var bot = new BotClient(m => m.Map<ILogger, ConsoleLogger>());
            bot.CommandService.Install(bot.DefaultCommands);
            bot.CommandService.Install(new Type[] { typeof(TestCommand) });

            bot.StartAsync(c =>
            {
                if (string.IsNullOrWhiteSpace(c))
                {
                    Console.WriteLine("Please enter a bot token:");
                    return Console.ReadLine();
                }
                return c;
            }).Wait();
            bot.UntilOffline.Wait();
        }

        class ConsoleLogger : Logger
        {
            protected override LogSeverity LogLevel => LogSeverity.Critical | LogSeverity.Debug | LogSeverity.Error | LogSeverity.Info | LogSeverity.Verbose;

            protected override void WriteLog(ILoggable entry)
            {
                Console.WriteLine(entry);
                base.WriteLog(entry);
            }
        }
    }
}
