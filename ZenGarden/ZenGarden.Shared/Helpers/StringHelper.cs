namespace ZenGarden.Shared.Helpers;

public static class StringHelper
{
    public static string NormalizeString(string? input)
    {
        return input?.Trim().ToLower() ?? string.Empty;
    }
    
    public static string FormatSecondsToTime(int seconds)
    {
        return TimeSpan.FromSeconds(seconds).ToString(@"hh\:mm\:ss");
    }
}