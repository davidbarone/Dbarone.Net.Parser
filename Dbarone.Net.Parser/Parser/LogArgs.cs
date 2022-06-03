namespace Dbarone.Net.Parser;

public class LogArgs
{
    public int NestingLevel { get; set; }
    public string Message { get; set; } = default!;
    public LogType LogType { get; set; } = LogType.Information;
}
