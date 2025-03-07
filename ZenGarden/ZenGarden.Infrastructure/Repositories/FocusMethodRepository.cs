using System.Diagnostics;
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

        var availableMethods = await _context.FocusMethod.Select(m => m.Name).ToListAsync();
        if (!availableMethods.Any()) return null;

        var descriptionText = string.IsNullOrWhiteSpace(taskDescription) ? "No description provided." : taskDescription;
        var prompt = $"Select the most suitable focus method for the task: {taskName}. " +
                     $"Description: {descriptionText}. " +
                     $"Available focus methods: {string.Join(", ", availableMethods)}. " +
                     $"Please choose one from the list.";

        Console.WriteLine($"[DEBUG] OpenAI Prompt: {prompt}");

        var requestBody = new
        {
            model = "gpt-4o",
            messages = new[] { new { role = "user", content = prompt } },
            max_tokens = 50
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        var stopwatch = Stopwatch.StartNew();
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"[ERROR] OpenAI API request failed: {ex.Message}", ex);
        }

        stopwatch.Stop();

        if (stopwatch.ElapsedMilliseconds > 3000)
            Console.WriteLine($"[WARNING] OpenAI API is slow: {stopwatch.ElapsedMilliseconds}ms");

        var responseBody = await response.Content.ReadAsStringAsync();
        JsonDocument jsonDoc;
        try
        {
            jsonDoc = JsonDocument.Parse(responseBody);
        }
        catch (JsonException ex)
        {
            throw new Exception($"[ERROR] Failed to parse OpenAI response: {ex.Message}", ex);
        }

        if (!jsonDoc.RootElement.TryGetProperty("choices", out var choices) || choices.GetArrayLength() == 0)
            throw new Exception("[ERROR] OpenAI response does not contain valid choices.");

        var firstChoice = choices[0];
        if (!firstChoice.TryGetProperty("message", out var message) ||
            !message.TryGetProperty("content", out var contentElement))
            throw new Exception("[ERROR] OpenAI response format is incorrect.");

        var suggestedMethodName = contentElement.GetString()?.Trim();
        if (string.IsNullOrEmpty(suggestedMethodName)) return null;

        var suggestedMethod = await _context.FocusMethod
            .FirstOrDefaultAsync(fm => fm.Name.ToLower() == suggestedMethodName.ToLower());

        if (suggestedMethod == null)
        {
            Console.WriteLine($"[WARNING] OpenAI suggested unknown focus method: {suggestedMethodName}");
            return null;
        }

        return suggestedMethod;
    }

    public async Task<FocusMethod?> GetByIdAsync(int focusMethodId)
    {
        return await _context.FocusMethod.FirstOrDefaultAsync(fm => fm.FocusMethodId == focusMethodId);
    }
}