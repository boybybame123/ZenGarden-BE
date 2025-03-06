using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Config;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class FocusMethodRepository : GenericRepository<FocusMethod>, IFocusMethodRepository
{
    private readonly ZenGardenContext _context;
    private readonly HttpClient _httpClient;

    public FocusMethodRepository(ZenGardenContext context, HttpClient httpClient,
        IOptions<OpenAiSettings> openAiSettings)
        : base(context)
    {
        _context = context;
        _httpClient = httpClient;
        var apiKey = openAiSettings.Value.ApiKey ??
                     throw new ArgumentNullException(nameof(openAiSettings), "OpenAI API Key is missing.");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    }

    public async Task<FocusMethod?> GetRecommendedMethodAsync(string taskName, string? taskDescription)
{
    if (string.IsNullOrWhiteSpace(taskName))
        throw new ArgumentException("Task name is required.", nameof(taskName));

    var availableMethods = await _context.FocusMethod.ToListAsync();
    if (availableMethods.Count == 0) return null;

    var prompt = $"Select the most suitable focus method for the task: {taskName}. " +
                 $"Description: {taskDescription}. " +
                 $"Available focus methods: {string.Join(", ", availableMethods.Select(m => m.Name))}. " +
                 $"Please choose one from the list.";

    var requestBody = new
    {
        model = "gpt-4o",
        messages = new[] { new { role = "user", content = prompt } },
        max_tokens = 50
    };

    var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

    HttpResponseMessage response;
    try
    {
        response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"[ERROR] OpenAI API request failed: {ex.Message}");
        return null; 
    }

    if (!response.IsSuccessStatusCode)
    {
        Console.WriteLine($"[ERROR] OpenAI API returned {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        return null;
    }

    var responseBody = await response.Content.ReadAsStringAsync();
    using var jsonDoc = JsonDocument.Parse(responseBody);
    if (!jsonDoc.RootElement.TryGetProperty("choices", out var choices) || choices.GetArrayLength() == 0)
        return null;

    var firstChoice = choices[0];
    if (!firstChoice.TryGetProperty("message", out var message) ||
        !message.TryGetProperty("content", out var contentElement))
        return null;

    var suggestedMethodName = contentElement.GetString()?.Trim();
    if (string.IsNullOrEmpty(suggestedMethodName))
        return null;

    var suggestedMethod = availableMethods.FirstOrDefault(fm => fm.Name.Equals(suggestedMethodName, StringComparison.CurrentCultureIgnoreCase));
    
    return suggestedMethod; 
}

    
    public async Task<FocusMethod?> GetByIdAsync(int focusMethodId)
    {
        return await _context.FocusMethod.FirstOrDefaultAsync(fm => fm.FocusMethodId == focusMethodId);
    }
    
}