namespace ZenGarden.Shared.Helpers;

public static class DateTimeHelper
{
    public static string ToIsoString(this DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
    }
}