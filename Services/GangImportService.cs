using System.Text.Json;
using MundaBattleReport.Models;

namespace MundaBattleReport.Services;

public class GangImportService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GangImportService> _logger;

    public GangImportService(HttpClient httpClient, ILogger<GangImportService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Fetch gang data from a Munda Manager share link or JSON input.
    /// </summary>
    public async Task<(bool Success, Gang? Gang, string ErrorMessage)> ImportGangAsync(string input)
    {
        // Try to parse as JSON first (user export fallback)
        if (input.TrimStart().StartsWith("{") || input.TrimStart().StartsWith("["))
        {
            return ParseGangJson(input);
        }

        // Try as URL (Munda Manager share link)
        if (Uri.TryCreate(input, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            return await FetchGangFromUrlAsync(uri);
        }

        return (false, null, "Invalid input: must be a valid URL or JSON");
    }

    private (bool Success, Gang? Gang, string ErrorMessage) ParseGangJson(string json)
    {
        try
        {
            var fighters = JsonSerializer.Deserialize<List<MundaFighter>>(json);
            if (fighters == null || fighters.Count == 0)
                return (false, null, "No fighters found in JSON");

            // Extract house from fighter data if available, default to "Unknown"
            var house = ExtractHouseFromFighters(fighters);
            var gangName = ExtractGangNameFromFighters(fighters);

            var gang = new Gang
            {
                Name = gangName,
                House = house,
                RawData = json,
                MmSource = "User JSON Export",
                ImportedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Successfully imported gang from JSON: {GangName} ({House})", gang.Name, gang.House);
            return (true, gang, "");
        }
        catch (JsonException ex)
        {
            _logger.LogWarning("Failed to parse JSON input: {Error}", ex.Message);
            return (false, null, $"Invalid JSON format: {ex.Message}");
        }
    }

    private async Task<(bool Success, Gang? Gang, string ErrorMessage)> FetchGangFromUrlAsync(Uri uri)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var response = await _httpClient.GetAsync(uri, HttpCompletionOption.ResponseContentRead, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Munda Manager link returned status {StatusCode}", response.StatusCode);
                return (false, null, $"Link returned error {response.StatusCode}. Link may be invalid or expired.");
            }

            var content = await response.Content.ReadAsStringAsync(cts.Token);

            // Try to extract JSON from response
            var fighters = ExtractFightersFromHtml(content);
            if (fighters == null || fighters.Count == 0)
            {
                return (false, null, "No fighter data found in Munda Manager page. Link may be invalid.");
            }

            var house = ExtractHouseFromFighters(fighters);
            var gangName = ExtractGangNameFromFighters(fighters);

            var rawJson = JsonSerializer.Serialize(fighters);
            var gang = new Gang
            {
                Name = gangName,
                House = house,
                RawData = rawJson,
                MmSource = uri.ToString(),
                ImportedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Successfully imported gang from Munda Manager: {GangName} ({House})", gang.Name, gang.House);
            return (true, gang, "");
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("Munda Manager request timed out");
            return (false, null, "Network timeout. Please check the link and try again.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning("Network error fetching Munda Manager data: {Error}", ex.Message);
            return (false, null, $"Network error: {ex.Message}");
        }
    }

    private List<MundaFighter>? ExtractFightersFromHtml(string html)
    {
        try
        {
            // Look for JSON embedded in HTML (common pattern for web apps)
            var jsonStart = html.IndexOf("\"fighters\":", StringComparison.OrdinalIgnoreCase);
            if (jsonStart == -1)
                return null;

            var bracketCount = 0;
            var inString = false;
            var escaped = false;
            var startPos = -1;

            for (var i = jsonStart + 11; i < html.Length; i++)
            {
                var ch = html[i];

                if (escaped)
                {
                    escaped = false;
                    continue;
                }

                if (ch == '\\')
                {
                    escaped = true;
                    continue;
                }

                if (ch == '"' && !escaped)
                {
                    inString = !inString;
                    continue;
                }

                if (!inString)
                {
                    if (ch == '[')
                    {
                        if (startPos == -1) startPos = i;
                        bracketCount++;
                    }
                    else if (ch == ']')
                    {
                        bracketCount--;
                        if (bracketCount == 0 && startPos != -1)
                        {
                            var jsonStr = html.Substring(startPos, i - startPos + 1);
                            return JsonSerializer.Deserialize<List<MundaFighter>>(jsonStr);
                        }
                    }
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Error extracting fighters from HTML: {Error}", ex.Message);
            return null;
        }
    }

    private string ExtractHouseFromFighters(List<MundaFighter> fighters)
    {
        if (fighters.Count == 0) return "Unknown";

        // Try to infer house from fighter class names or other metadata
        // For now, return "Unknown" and let user verify/correct
        return "Unknown";
    }

    private string ExtractGangNameFromFighters(List<MundaFighter> fighters)
    {
        // Try to find a leader/champion as gang identifier
        var leader = fighters.FirstOrDefault(f => f.FighterType == "Leader");
        if (leader?.FighterName != null)
            return $"{leader.FighterName}'s Gang";

        return "Imported Gang";
    }
}
