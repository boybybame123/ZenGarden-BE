namespace ZenGarden.Domain.Config;

public abstract class OpenAiSettings
{
    public string ApiKey { get; set; } = string.Empty;
}