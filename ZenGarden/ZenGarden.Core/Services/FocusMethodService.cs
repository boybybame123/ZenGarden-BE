using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
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

public partial class FocusMethodService : IFocusMethodService
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

        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    }

    public async Task<FocusMethodDto> SuggestFocusMethodAsync(SuggestFocusMethodDto dto)
    {
        try
        {
            var availableMethodNames = await _focusMethodRepository.GetMethodNamesAsync();
            var isDatabaseEmpty = availableMethodNames.Count == 0;

            var prompt = isDatabaseEmpty
                ? $"Given the task details:\n\n- **Task Name**: {dto.TaskName}\n- **Task Description**: {dto.TaskDescription}\n- **Start Date**: {dto.StartDate:yyyy-MM-dd}\n- **End Date**: {dto.EndDate:yyyy-MM-dd}\n\nSuggest a focus method optimized for this task. Return the result in exactly this format:\n\nName - MinDuration - MaxDuration - MinBreak - MaxBreak - DefaultDuration - DefaultBreak.\n\nAll values are in minutes. Do not explain, just return the result."
                : $"Given the task details:\n\n- **Task Name**: {dto.TaskName}\n- **Task Description**: {dto.TaskDescription}\n- **Start Date**: {dto.StartDate:yyyy-MM-dd}\n- **End Date**: {dto.EndDate:yyyy-MM-dd}\n\nChoose the most suitable focus method from the list: {string.Join(", ", availableMethodNames)}.\nIf none are suitable, suggest one. Return the result in exactly this format:\n\nName - MinDuration - MaxDuration - MinBreak - MaxBreak - DefaultDuration - DefaultBreak.\n\nAll values are in minutes. Do not explain, just return the result.";
            
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
                MinDuration = ExtractNumber(parts[1], 15),
                MaxDuration = ExtractNumber(parts[2], 120),
                MinBreak = ExtractNumber(parts[3], 5),
                MaxBreak = ExtractNumber(parts[4], 30),
                DefaultDuration = ExtractNumber(parts[5], 45),
                DefaultBreak = ExtractNumber(parts[6], 10),
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
            throw new InvalidOperationException(
                $"Failed to save new focus method to database: {ex.InnerException?.Message}", ex);
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
                    model = "google/gemma-3-27b-it",
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    },
                    max_tokens = 100
                };

                var jsonPayload = JsonSerializer.Serialize(requestBody);
                Console.WriteLine($"[ZenGarden] OpenAI Request: {jsonPayload}");

                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var response =
                    await _httpClient.PostAsync("https://api.deepinfra.com/v1/openai/chat/completions", content);

                _lastRequestTime = DateTime.UtcNow;

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    Console.WriteLine(
                        $"[ZenGarden] OpenAI Rate Limit hit. Retrying... Attempt {attempt + 1}/{maxRetries}");
                    if (attempt == maxRetries - 1)
                        throw new HttpRequestException("OpenAI API is overloaded. Try again later.");

                    attempt++;
                    await Task.Delay(2000);
                    continue;
                }

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"OpenAI API error: {response.StatusCode} - {errorMessage}");
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[ZenGarden] OpenAI Response: {responseBody}");

                using var jsonDoc = JsonDocument.Parse(responseBody);
                var result = jsonDoc.RootElement.GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString()?.Trim() ?? "";

                _cache.Set(prompt, result, TimeSpan.FromMinutes(10));
                return result;
            }
            catch (HttpRequestException ex) when (attempt < maxRetries - 1)
            {
                Console.WriteLine($"[ZenGarden] OpenAI Request Failed (Attempt {attempt + 1}): {ex.Message}");
                attempt++;
                await Task.Delay(2000);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"[ZenGarden] OpenAI Response Parse Error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ZenGarden] Unexpected OpenAI API Error: {ex.Message}");
                throw;
            }
            finally
            {
                RateLimitSemaphore.Release();
            }
        }

        throw new HttpRequestException("The OpenAI API request failed after multiple attempts.");
    }
    
    private static int ExtractNumber(string input, int defaultValue)
    {
        var match = MyRegex().Match(input);
        return match.Success ? int.Parse(match.Value) : defaultValue;
    }

    [GeneratedRegex(@"\d+")]
    private static partial Regex MyRegex();
}