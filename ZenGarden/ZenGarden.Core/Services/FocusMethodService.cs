using System.Globalization;
using System.Net;
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
    private static readonly SemaphoreSlim RateLimitSemaphore = new(1, 1);
    private static readonly TimeSpan RateLimitInterval = TimeSpan.FromSeconds(2);
    private static DateTime _lastRequestTime = DateTime.MinValue;
    private readonly IMemoryCache _cache;
    private readonly IFocusMethodRepository _focusMethodRepository;
    private readonly HttpClient _httpClient;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public FocusMethodService(IFocusMethodRepository focusMethodRepository, HttpClient httpClient, IMapper mapper,
        IOptions<OpenAiSettings> openAiSettings, IMemoryCache cache, IUnitOfWork unitOfWork)
    {
        _focusMethodRepository = focusMethodRepository;
        _httpClient = httpClient;
        _mapper = mapper;
        _cache = cache;
        _unitOfWork = unitOfWork;

        var apiKey = openAiSettings.Value.ApiKey;
        if (string.IsNullOrEmpty(apiKey))
            throw new InvalidOperationException("OpenAI API Key is missing.");

        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    }

    public async Task<FocusMethodWithReasonDto> SuggestFocusMethodAsync(SuggestFocusMethodDto dto)
    {
        try
        {
            var prompt = $"""
                          Given the task details:

                          - **Task Name**: {dto.TaskName}
                          - **Task Description**: {dto.TaskDescription}
                          - **Total Duration**: {dto.TotalDuration} minutes
                          - **Start Date**: {dto.StartDate:yyyy-MM-dd}
                          - **End Date**: {dto.EndDate:yyyy-MM-dd}

                          Suggest the most suitable focus method from these options based on the task characteristics:

                          1. Pomodoro Classic (25-25-5-5): Best for standard tasks requiring moderate focus
                          2. Flow Time (60-120-10-20): Best for deep work and complex tasks
                          3. Quick Burst (10-20-2-5): Best for short, simple tasks
                          4. Deep Work (90-180-20-45): Best for complex, creative tasks
                          5. Sprint Method (30-60-5-15): Best for project sprints and deadlines
                          6. Power Hour (45-75-5-15): Best for high-intensity tasks
                          7. Micro Focus (5-15-1-3): Best for very short tasks
                          8. Marathon Mode (120-240-15-30): Best for long-duration tasks
                          9. Study Block (40-60-5-15): Best for learning and studying
                          10. Creative Flow (45-90-10-25): Best for creative work

                          Consider these factors when suggesting:
                          - Task complexity and type
                          - Total duration available
                          - Need for breaks
                          - Level of focus required
                          - Whether it's creative or analytical work

                          Return the result in EXACTLY the following format:

                          1. First line: Name - MinWorkDuration - MaxWorkDuration - MinBreak - MaxBreak - DefaultWorkDuration - DefaultBreak - XpMultiplier - Description

                          2. Second part (on new lines): Reason: [explain in 1-3 sentences why this method is suitable for this task]

                          Example:
                          Quick Burst - 10 - 25 - 1 - 5 - 20 - 5 - 1.1 - A short method for tight schedules with a small break.
                          Reason: This method fits well within the 30-minute window, allowing one full cycle of focused work and recovery.
                          """;
            var aiResponse = await CallOpenAiApi(prompt);
            if (string.IsNullOrWhiteSpace(aiResponse))
                throw new InvalidOperationException("OpenAI returned an empty response.");

            var lines = aiResponse.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            string reason;
            if (lines.Length > 1)
            {
                var remainingLines = lines.Skip(1).ToArray();

                var firstReasonLine = remainingLines[0].Trim();
                if (firstReasonLine.StartsWith("Reason:", StringComparison.OrdinalIgnoreCase))
                    remainingLines[0] = firstReasonLine.Substring("Reason:".Length).Trim();

                reason = string.Join(" ", remainingLines).Trim();
            }
            else
            {
                reason = $"This method is recommended by AI based on the characteristics of the task '{dto.TaskName}'.";
            }

            if (string.IsNullOrWhiteSpace(reason))
                reason = $"This method is recommended by AI based on the characteristics of the task '{dto.TaskName}'.";


            var parts = lines[0].Split('-', 9).Select(p => p.Trim()).ToArray();
            if (parts.Length != 9)
                throw new InvalidDataException($"Invalid method format: {lines[0]}");

            var suggestedMethodName = parts[0];
            var newMethod = new FocusMethod
            {
                Name = suggestedMethodName,
                MinDuration = ExtractNumber(parts[1], 15),
                MaxDuration = ExtractNumber(parts[2], 120),
                MinBreak = ExtractNumber(parts[3], 5),
                MaxBreak = ExtractNumber(parts[4], 30),
                DefaultDuration = ExtractNumber(parts[5], 45),
                DefaultBreak = ExtractNumber(parts[6], 10),
                XpMultiplier = ExtractDouble(parts[7], 1.0),
                Description = parts.Length > 8 ? parts[8] : "",
                IsActive = true
            };

            var existing = await _focusMethodRepository.GetByNameAsync(suggestedMethodName);

            if (existing == null)
            {
                await _focusMethodRepository.CreateAsync(newMethod);
            }
            else
            {
                if (existing.MinDuration != newMethod.MinDuration ||
                    existing.MaxDuration != newMethod.MaxDuration ||
                    existing.MinBreak != newMethod.MinBreak ||
                    existing.MaxBreak != newMethod.MaxBreak ||
                    existing.DefaultDuration != newMethod.DefaultDuration ||
                    existing.DefaultBreak != newMethod.DefaultBreak)
                {
                    existing.MinDuration = newMethod.MinDuration;
                    existing.MaxDuration = newMethod.MaxDuration;
                    existing.MinBreak = newMethod.MinBreak;
                    existing.MaxBreak = newMethod.MaxBreak;
                    existing.DefaultDuration = newMethod.DefaultDuration;
                    existing.DefaultBreak = newMethod.DefaultBreak;
                    existing.XpMultiplier = newMethod.XpMultiplier;
                    existing.Description = newMethod.Description;
                    existing.IsActive = true;
                    existing.UpdatedAt = DateTime.UtcNow;

                    _focusMethodRepository.Update(existing);
                    await _unitOfWork.CommitAsync();
                }
            }

            await _unitOfWork.CommitAsync();

            var resultMethod = existing ?? newMethod;

            var resultDto = _mapper.Map<FocusMethodWithReasonDto>(resultMethod);
            resultDto.Reason = reason;
            return resultDto;
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException("Failed to connect to OpenAI API. Please try again later.", ex);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Failed to parse OpenAI response. The API returned unexpected data.",
                ex);
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

    public async Task<List<FocusMethodDto>> GetAllFocusMethodsAsync()
    {
        var focusMethods = await _focusMethodRepository.GetAllAsync();
        return _mapper.Map<List<FocusMethodDto>>(focusMethods);
    }

    private async Task<string?> CallOpenAiApi(string prompt)
    {
        if (_cache.TryGetValue(prompt, out string? cachedResponse)) return cachedResponse;

        const int maxRetries = 3;
        var attempt = 0;

        while (attempt < maxRetries)
        {
            await RateLimitSemaphore.WaitAsync();
            try
            {
                var timeSinceLastRequest = DateTime.UtcNow - _lastRequestTime;
                if (timeSinceLastRequest < RateLimitInterval)
                    await Task.Delay(RateLimitInterval - timeSinceLastRequest);

                var requestBody = new
                {
                    model = "google/gemma-3-27b-it",
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    },
                    max_tokens = 200
                };

                var jsonPayload = JsonSerializer.Serialize(requestBody);
                Console.WriteLine($"[ZenGarden] OpenAI Request: {jsonPayload}");

                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var response =
                    await _httpClient.PostAsync("https://api.deepinfra.com/v1/openai/chat/completions", content);

                _lastRequestTime = DateTime.UtcNow;

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
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
        if (match.Success && int.TryParse(match.Value, out var result)) return result > 0 ? result : defaultValue;

        return defaultValue;
    }

    private static double ExtractDouble(string input, double defaultValue)
    {
        return double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out var result)
            ? result
            : defaultValue;
    }

    [GeneratedRegex(@"\d+")]
    private static partial Regex MyRegex();
}