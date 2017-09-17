﻿using System;
using System.Reflection;

namespace TitanBot.Services.Command
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public class RawArgumentsAttribute : Attribute
    {
        public static bool ExistsOn(ParameterInfo info)
            => info.GetCustomAttribute<RawArgumentsAttribute>() != null;
    }
}
