namespace Dbarone.Net.Parser;
using System;

public interface ILoggable
{
    Action<object, LogArgs> LogHandler { get; set; }
}

