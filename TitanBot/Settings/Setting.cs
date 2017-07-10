﻿using TitanBot.Storage;

namespace TitanBot.Settings
{
    class Setting : IDbRecord
    {
        public ulong Id { get; set; }
        public string Serialized { get; set; }
    }
}