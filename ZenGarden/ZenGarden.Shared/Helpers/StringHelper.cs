namespace ZenGarden.Shared.Helpers;

public static class StringHelper
{
    public static string NormalizeString(string? input)
    {
        return input?.Trim().ToLower() ?? string.Empty;
    }
}
