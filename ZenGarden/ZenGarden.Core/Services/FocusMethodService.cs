using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Config;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Services;

public class FocusMethodService : IFocusMethodService
{
    private readonly IFocusMethodRepository _focusMethodRepository;
    private readonly HttpClient _httpClient;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;
    private static readonly SemaphoreSlim RateLimitSemaphore = new(1, 1);
    private static readonly TimeSpan RateLimitInterval = TimeSpan.FromSeconds(2);
    private static DateTime _lastRequestTime = DateTime.MinValue;

    public FocusMethodService(IFocusMethodRepository focusMethodRepository, HttpClient httpClient, IMapper mapper,
        IOptions<OpenAiSettings> openAiSettings, IMemoryCache cache)
    {
        _focusMethodRepository = focusMethodRepository;
        _httpClient = httpClient;
        _mapper = mapper;
        _cache = cache;

        var apiKey = openAiSettings.Value.ApiKey;
        if (string.IsNullOrEmpty(apiKey))
            throw new InvalidOperationException("OpenAI API Key is missing.");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    }

    public async Task<FocusMethodDto> SuggestFocusMethodAsync(SuggestFocusMethodDto dto)
    {
        try
        {
            var availableMethodNames = await _focusMethodRepository.GetMethodNamesAsync();
            var isDatabaseEmpty = availableMethodNames.Count == 0;

            var prompt = isDatabaseEmpty
                ? $"Given the task '{dto.TaskName}', suggest a *new unique focus method* with MinDuration, MaxDuration, MinBreak, MaxBreak, DefaultDuration, DefaultBreak. " +
                  $"Return in format: Name - MinDuration - MaxDuration - MinBreak - MaxBreak - DefaultDuration - DefaultBreak."
                : $"Given the task '{dto.TaskName}', choose the most suitable focus method from the list: {string.Join(", ", availableMethodNames)}. " +
                  $"If none are suitable, suggest a new one. " +
                  $"Return in format: Name - MinDuration - MaxDuration - MinBreak - MaxBreak - DefaultDuration - DefaultBreak.";

            var aiResponse = await CallOpenAiApi(prompt);
            if (string.IsNullOrWhiteSpace(aiResponse))
                throw new InvalidOperationException("OpenAI returned an empty response.");

            var parts = aiResponse.Split('-').Select(p => p.Trim()).ToArray();
            if (parts.Length != 7)
                throw new InvalidDataException($"Invalid AI response format: {aiResponse}");

            var suggestedMethodName = parts[0];

            var existingMethod = await _focusMethodRepository.GetByNameAsync(suggestedMethodName);
            if (existingMethod != null) return _mapper.Map<FocusMethodDto>(existingMethod);

            var newMethod = new FocusMethod
            {
                Name = suggestedMethodName,
                MinDuration = int.TryParse(parts[1], out var minDur) ? minDur : 15,
                MaxDuration = int.TryParse(parts[2], out var maxDur) ? maxDur : 120,
                MinBreak = int.TryParse(parts[3], out var minBrk) ? minBrk : 5,
                MaxBreak = int.TryParse(parts[4], out var maxBrk) ? maxBrk : 30,
                DefaultDuration = int.TryParse(parts[5], out var defDur) ? defDur : 45,
                DefaultBreak = int.TryParse(parts[6], out var defBrk) ? defBrk : 10,
                IsActive = true
            };

            await _focusMethodRepository.CreateAsync(newMethod);
            return _mapper.Map<FocusMethodDto>(newMethod);
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException("Failed to connect to OpenAI API. Please try again later.", ex);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Failed to parse OpenAI response. The API returned unexpected data.", ex);
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException($"Failed to save new focus method to database: {ex.InnerException?.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An unexpected error occurred.", ex);
        }
    }

    private async Task<string?> CallOpenAiApi(string prompt)
{
    if (_cache.TryGetValue(prompt, out string? cachedResponse))
    {
        return cachedResponse;
    }

    const int maxRetries = 3;
    int attempt = 0;

    while (attempt < maxRetries)
    {
        await RateLimitSemaphore.WaitAsync();
        try
        {
            var timeSinceLastRequest = DateTime.UtcNow - _lastRequestTime;
            if (timeSinceLastRequest < RateLimitInterval)
            {
                await Task.Delay(RateLimitInterval - timeSinceLastRequest);
            }

            var requestBody = new
            {
                model = "gpt-4o",
                messages = new[] { new { role = "user", content = prompt } },
                max_tokens = 40
            };

            var jsonPayload = JsonSerializer.Serialize(requestBody);
            Console.WriteLine($"[ZenGarden] OpenAI Request: {jsonPayload}");

            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

            _lastRequestTime = DateTime.UtcNow;

            var responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[ZenGarden] OpenAI Response: {responseBody}");

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                Console.WriteLine($"[ZenGarden] OpenAI Rate Limit hit. Retrying... Attempt {attempt + 1}/{maxRetries}");
                if (attempt == maxRetries - 1)
                    throw new InvalidOperationException("The OpenAI API is currently overloaded. Please try again later.");

                attempt++;
                await Task.Delay(2000); 
                continue;
            }

            response.EnsureSuccessStatusCode();

            using var jsonDoc = JsonDocument.Parse(responseBody);
            var result = jsonDoc.RootElement.GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString()?.Trim() ?? "";

            _cache.Set(prompt, result, TimeSpan.FromMinutes(10));

            return result;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"[ZenGarden] OpenAI Request Failed: {ex.Message}");
            if (attempt == maxRetries - 1)
                throw new InvalidOperationException("Failed to connect to OpenAI API. Please check your network connection and try again.", ex);
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"[ZenGarden] OpenAI Response Parse Error: {ex.Message}");
            throw new InvalidOperationException("Failed to parse OpenAI API response. Unexpected data format.", ex);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ZenGarden] Unexpected OpenAI API Error: {ex.Message}");
            throw new InvalidOperationException("An unexpected error occurred while calling OpenAI API.", ex);
        }
        finally
        {
            RateLimitSemaphore.Release();
        }

        attempt++;
    }

    throw new InvalidOperationException("The OpenAI API request failed after multiple attempts.");
}
}