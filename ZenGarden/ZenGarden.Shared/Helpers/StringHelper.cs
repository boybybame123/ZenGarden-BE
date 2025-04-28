namespace ZenGarden.Shared.Helpers;

public static class StringHelper
{
    public static string NormalizeString(string? input)
    {
        return input?.Trim().ToLower() ?? string.Empty;
    }

    public static string FormatSecondsToTime(int seconds)
    {
        var timeSpan = TimeSpan.FromSeconds(seconds);
        var totalHours = (int)timeSpan.TotalHours;
        return $"{totalHours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
    }
}