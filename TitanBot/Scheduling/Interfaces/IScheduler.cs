﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TitanBot.Scheduling
{
    public interface IScheduler
    {
        bool Enabled { get; set; }
        ulong Queue<T>(ulong userId, ulong? guildID, DateTime from, TimeSpan? period = null, DateTime? to = null, ulong? message = null, ulong? channel = null, string data = null)
            where T : ISchedulerCallback;
        ISchedulerRecord[] Cancel<T>(ulong? guildId, ulong? userId)
            where T : ISchedulerCallback;
        ISchedulerRecord Cancel(ulong id);
        ISchedulerRecord[] Cancel(IEnumerable<ulong> ids);
        ValueTask<int> PruneBefore(DateTime date);
        int ActiveCount();
        ISchedulerRecord GetMostRecent<T>(ulong? guildId, ulong? userId)
            where T : ISchedulerCallback;
    }
}
