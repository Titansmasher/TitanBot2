﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanBot2;
using TitanBot2.Common;

namespace TitanBotConsole
{
    class Program
    {
        private TitanBot _bot;

        static void Main(string[] args)
            => new Program().Start().GetAwaiter().GetResult();

        public async Task Start()
        {
            _bot = new TitanBot();

            _bot.Logger.HandleLog += e => Console.Out.WriteLineAsync(e.ToString());
            _bot.LoggedOut += async () =>
            {
                await Console.Out.WriteLineAsync("Logged out");
                await Console.In.ReadLineAsync();
                Environment.Exit(0);
            };

            if (!await _bot.StartAsync())
            {
                var config = Configuration.Instance;
                Console.WriteLine("Please enter the bot token to use:");
                config.Token = Console.ReadLine();
                config.SaveJson();
                if (!await _bot.StartAsync())
                {
                    Console.WriteLine("Startup failed. Press ENTER to exit");
                    Console.ReadLine();
                    return;
                }
            }

            await Task.Delay(-1);
        }
    }
}