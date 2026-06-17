namespace SimpleMCP.Server.Tools;

internal static class DebugTool
{
    internal async static Task<string> ExecuteDebugToolAsync(string inputText)
    {
        await Task.CompletedTask;

        return $"Hello World {inputText}";
    }
}
