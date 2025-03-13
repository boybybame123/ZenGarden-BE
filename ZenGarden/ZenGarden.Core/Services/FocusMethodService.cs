using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
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

    public FocusMethodService(IFocusMethodRepository focusMethodRepository, HttpClient httpClient, IMapper mapper,
        IOptions<OpenAiSettings> openAiSettings)
    {
        _focusMethodRepository = focusMethodRepository;
        _httpClient = httpClient;
        _mapper = mapper;

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
            ? $"Given the task '{dto.TaskName}', suggest a **new unique focus method** with MinDuration, MaxDuration, MinBreak, MaxBreak, DefaultDuration, DefaultBreak. " +
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
    
    private async Task<string> CallOpenAiApi(string prompt)
    {
        var requestBody = new
        {
            model = "gpt-4o",
            messages = new[] { new { role = "user", content = prompt } },
            max_tokens = 100
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(responseBody);
        return jsonDoc.RootElement.GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString()?.Trim() ?? "";
    }
}