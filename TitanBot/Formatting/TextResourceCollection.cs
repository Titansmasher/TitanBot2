﻿using System.Collections.Generic;
using System.Linq;
using TitanBot.Commands;

namespace TitanBot.Formatting
{
    class TextResourceCollection : ITextResourceCollection
    {
        private Dictionary<string, (string defaultText, string langText)> Values { get; }
        private ValueFormatter Formatter { get; }
        public double Coverage { get; }

        public string this[string key] => GetResource(key);

        public TextResourceCollection(double coverage, ValueFormatter valueFormatter, Dictionary<string, (string defaultText, string langText)> values)
        {
            Coverage = coverage;
            Values = values;
            Formatter = valueFormatter;
        }

        public string GetResource(string key)
        {
            if (key == null)
                return null;
            if (key.Contains(' '))
                return key;
            if (!Values.ContainsKey(key.ToUpper()))
                return key;
            var val = Values[key.ToUpper()];
            return val.langText ?? val.defaultText ?? key;
        }

        public string Format(string key, params object[] items)
            => string.Format(GetResource(key), items.Select(i => Formatter.Beautify(i)).ToArray());

        public string Format(string key, ReplyType replyType, params object[] items)
            => GetReplyType(replyType) + Format(key, items);

        public string GetResource(string key, ReplyType replyType)
            => GetReplyType(replyType) + GetResource(key);

        public string GetReplyType(ReplyType replyType)
        {
            switch (replyType)
            {
                case ReplyType.Success:
                    return GetResource("REPLYTYPE_SUCCESS");
                case ReplyType.Error:
                    return GetResource("REPLYTYPE_ERROR");
                case ReplyType.Info:
                    return GetResource("REPLYTYPE_INFO");
                default:
                    return "";
            }
        }
    }
}
