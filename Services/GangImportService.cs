using System.Text.Json;
using MundaBattleReport.Models;
using PuppeteerSharp;
using Microsoft.Extensions.Configuration;

namespace MundaBattleReport.Services;

public class GangImportService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GangImportService> _logger;
    private readonly IConfiguration _config;

    public GangImportService(HttpClient httpClient, ILogger<GangImportService> logger, IConfiguration config)
    {
        _httpClient = httpClient;
        _logger = logger;
        _config = config;
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
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var fighters = JsonSerializer.Deserialize<List<MundaFighter>>(json, options);
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
            // Use Puppeteer to render the page and extract data
            _logger.LogInformation("Fetching Munda Manager gang page using Puppeteer: {Uri}", uri);
            var mundaUsername = _config["MundaManager:Username"];
            var mundaPassword = _config["MundaManager:Password"];

            var fighters = await FetchGangDataWithPuppeteerAsync(uri, mundaUsername, mundaPassword);

            if (fighters == null || fighters.Count == 0)
            {
                _logger.LogWarning("No fighter data extracted from Munda Manager page");
                return (false, null, "Could not read gang data from the Munda Manager link. Please verify the link is correct.");
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

            _logger.LogInformation("Successfully imported gang from Munda Manager: {GangName} ({House}) with {FighterCount} fighters", gang.Name, gang.House, fighters.Count);
            return (true, gang, "");
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("Munda Manager request timed out");
            return (false, null, "Network timeout fetching Munda Manager page. Please check your connection and try again.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Error fetching gang data from Munda Manager: {Error}", ex.Message);
            var errorMsg = ex.Message.Contains("403") || ex.Message.Contains("401")
                ? "Access denied - are you logged into Munda Manager? Make sure the share link is from an authenticated session."
                : $"Error reading Munda Manager page: {ex.Message}";
            return (false, null, errorMsg);
        }
    }

    private static IBrowser? _browser;
    private static readonly SemaphoreSlim _browserLock = new SemaphoreSlim(1, 1);

    private async Task<List<MundaFighter>?> FetchGangDataWithPuppeteerAsync(Uri uri, string? username = null, string? password = null)
    {
        try
        {
            // Ensure browser is initialized (one-time cost)
            await _browserLock.WaitAsync();
            try
            {
                if (_browser == null)
                {
                    _logger.LogInformation("Initializing Puppeteer browser...");
                    var browserFetcher = new BrowserFetcher();
                    await browserFetcher.DownloadAsync();

                    _browser = await Puppeteer.LaunchAsync(new LaunchOptions
                    {
                        Headless = true,
                        Args = new[]
                        {
                            "--no-sandbox",
                            "--disable-setuid-sandbox",
                            "--disable-dev-shm-usage"
                        }
                    });
                }
            }
            finally
            {
                _browserLock.Release();
            }

            await using var page = await _browser.NewPageAsync();
            page.DefaultTimeout = 10000;
            page.DefaultNavigationTimeout = 10000;

            // Try to access the page directly first (share links may be publicly accessible)
            // Skip login - Munda Manager has CAPTCHA which blocks automation

            _logger.LogInformation("Navigating to Munda Manager gang page: {Uri}", uri);

            try
            {
                await page.GoToAsync(uri.ToString(), WaitUntilNavigation.Networkidle2);
            }
            catch (WaitTaskTimeoutException)
            {
                _logger.LogWarning("Navigation timeout, continuing with page content");
                // Page may have loaded enough content
            }

            // Wait for fighter content to appear
            try
            {
                await page.WaitForFunctionAsync(
                    "() => document.body.innerText.includes('Fighter') || document.body.innerText.includes('WS')",
                    new WaitForFunctionOptions { Timeout = 5000 }
                );
            }
            catch
            {
                _logger.LogWarning("Timeout waiting for fighter content");
            }

            // Extract fighter data using Munda Manager's specific DOM structure
            var fightersJson = await page.EvaluateFunctionAsync<string?>(@"
                () => {
                    const fighters = [];

                    // Munda Manager wraps each fighter in an <a> tag with href=""/fighter/{guid}""
                    const fighterLinks = document.querySelectorAll('a[href*=""/fighter/""]');

                    fighterLinks.forEach(link => {
                        // Find the fighter card div within this link
                        const card = link.querySelector('div[class*=""fighter-card-bg""]');
                        if (!card) return;

                        // Extract fighter name from fancy-print-keep-color-heading
                        const nameEl = card.querySelector('.fancy-print-keep-color-heading');
                        const fighterName = nameEl?.textContent?.trim() || '';

                        // Extract fighter type from fancy-print-keep-color-subtitle
                        const typeEl = card.querySelector('.fancy-print-keep-color-subtitle');
                        const fighterTypeRaw = typeEl?.textContent?.trim() || 'Ganger';

                        // Parse type (e.g., ""Road Captain (Leader)"" -> type=Leader, class=Road Captain)
                        const typeMatch = fighterTypeRaw.match(/(.+?)\\s*\\((.+?)\\)/);
                        const fighterClass = typeMatch ? typeMatch[1].trim() : '';
                        const fighterType = typeMatch ? typeMatch[2].trim() : fighterTypeRaw;

                        // Extract credits from the credits badge
                        let credits = 0;
                        const creditSpans = Array.from(card.querySelectorAll('span')).find(el => el.textContent?.includes('Credits'));
                        if (creditSpans) {
                            const creditValue = creditSpans.previousElementSibling?.textContent?.trim() || '';
                            credits = parseInt(creditValue) || 0;
                        }

                        // Extract stats from the first table (stat block)
                        const statsTable = card.querySelector('table:not(.table-weapons)');
                        const stats = extractStatsFromTable(statsTable);

                        // Extract weapons from the weapons table
                        const weaponsTable = card.querySelector('table.table-weapons');
                        const weapons = extractWeaponsFromTable(weaponsTable);

                        // Extract wargear and skills
                        const wargearText = card.innerText.match(/Wargear\\s+(.+?)(?=Skills|$)/)?.[1]?.trim() || '';
                        const skillsText = card.innerText.match(/Skills\\s+(.+?)(?=Special|Wargear|$)/)?.[1]?.trim() || '';

                        if (fighterName) {
                            fighters.push({
                                id: link.getAttribute('href')?.replace('/fighter/', '') || '',
                                fighterName: fighterName,
                                fighterType: fighterType,
                                fighterClass: fighterClass,
                                credits: credits,
                                movement: stats.m || 0,
                                weaponSkill: stats.ws || 0,
                                ballisticSkill: stats.bs || 0,
                                strength: stats.s || 0,
                                toughness: stats.t || 0,
                                wounds: stats.w || 0,
                                initiative: stats.i || 0,
                                attacks: stats.a || 0,
                                leadership: stats.ld || 0,
                                cool: stats.cl || 0,
                                willpower: stats.wil || 0,
                                intelligence: stats.int || 0,
                                weapons: weapons,
                                wargear: wargearText.split(',').map(w => ({ name: w.trim() })).filter(w => w.name),
                                effects: { active: skillsText.split(',').map(s => s.trim()).filter(s => s) }
                            });
                        }
                    });

                    return fighters.length > 0 ? JSON.stringify(fighters) : null;

                    function extractStatsFromTable(table) {
                        if (!table) return {};
                        const stats = {};
                        const headerCells = table.querySelectorAll('thead th');
                        const valueCells = table.querySelectorAll('tbody td');

                        headerCells.forEach((cell, idx) => {
                            const label = cell.textContent?.trim().toLowerCase() || '';
                            const value = valueCells[idx]?.textContent?.trim() || '';
                            if (label === 'm') stats.m = parseInt(value) || 0;
                            if (label === 'ws') stats.ws = parseInt(value) || 0;
                            if (label === 'bs') stats.bs = parseInt(value) || 0;
                            if (label === 's') stats.s = parseInt(value) || 0;
                            if (label === 't') stats.t = parseInt(value) || 0;
                            if (label === 'w') stats.w = parseInt(value) || 0;
                            if (label === 'i') stats.i = parseInt(value) || 0;
                            if (label === 'a') stats.a = parseInt(value) || 0;
                            if (label === 'ld') stats.ld = parseInt(value) || 0;
                            if (label === 'cl') stats.cl = parseInt(value) || 0;
                            if (label === 'wil') stats.wil = parseInt(value) || 0;
                            if (label === 'int') stats.int = parseInt(value) || 0;
                        });

                        return stats;
                    }

                    function extractWeaponsFromTable(table) {
                        if (!table) return [];
                        const weapons = [];
                        const rows = table.querySelectorAll('tbody tr');

                        rows.forEach(row => {
                            const nameCell = row.querySelector('td:first-child');
                            if (nameCell) {
                                weapons.push({
                                    name: nameCell.textContent?.trim() || 'Unknown Weapon',
                                    type: 'ranged'
                                });
                            }
                        });

                        return weapons;
                    }
                }
            ");

            if (!string.IsNullOrEmpty(fightersJson))
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var fighters = JsonSerializer.Deserialize<List<MundaFighter>>(fightersJson, options);
                if (fighters?.Count > 0)
                {
                    _logger.LogInformation("Extracted {FighterCount} fighters from page", fighters.Count);
                    return fighters;
                }
            }

            _logger.LogWarning("No fighters extracted from Munda Manager page");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Puppeteer extraction failed: {Error}", ex.Message);
            return null;
        }
    }

    private List<MundaFighter>? ExtractFightersFromHtml(string html)
    {
        try
        {
            // Strategy 1: Try __NEXT_DATA__ (Next.js apps like Munda Manager)
            var nextDataMatch = System.Text.RegularExpressions.Regex.Match(
                html,
                @"<script\s+id=""__NEXT_DATA__""[^>]*>(.+?)</script>",
                System.Text.RegularExpressions.RegexOptions.Singleline);

            if (nextDataMatch.Success)
            {
                var nextDataJson = nextDataMatch.Groups[1].Value;
                var nextData = JsonSerializer.Deserialize<System.Text.Json.JsonElement>(nextDataJson);

                // Try to find fighters in props
                if (nextData.TryGetProperty("props", out var props) &&
                    props.TryGetProperty("pageProps", out var pageProps) &&
                    pageProps.TryGetProperty("fighters", out var fighters))
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<List<MundaFighter>>(fighters.GetRawText(), options);
                }

                // Alternative: search for fighters anywhere in the object
                var fightersJson = FindFightersInJson(nextData);
                if (fightersJson != null)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<List<MundaFighter>>(fightersJson, options);
                }
            }

            // Strategy 2: Look for direct "fighters" JSON in HTML
            var jsonStart = html.IndexOf("\"fighters\":", StringComparison.OrdinalIgnoreCase);
            if (jsonStart != -1)
            {
                return ExtractFightersArray(html, jsonStart + 11);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Error extracting fighters from HTML: {Error}", ex.Message);
            return null;
        }
    }

    private List<MundaFighter>? ExtractFightersArray(string html, int startIdx)
    {
        var bracketCount = 0;
        var inString = false;
        var escaped = false;
        var startPos = -1;

        for (var i = startIdx; i < html.Length; i++)
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
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        return JsonSerializer.Deserialize<List<MundaFighter>>(jsonStr, options);
                    }
                }
            }
        }

        return null;
    }

    private string? FindFightersInJson(System.Text.Json.JsonElement element, int depth = 0)
    {
        if (depth > 10) return null; // Prevent infinite recursion

        if (element.ValueKind == System.Text.Json.JsonValueKind.Array)
        {
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var fighters = JsonSerializer.Deserialize<List<MundaFighter>>(element.GetRawText(), options);
                if (fighters?.Count > 0)
                    return element.GetRawText();
            }
            catch { }
        }

        if (element.ValueKind == System.Text.Json.JsonValueKind.Object)
        {
            foreach (var prop in element.EnumerateObject())
            {
                var result = FindFightersInJson(prop.Value, depth + 1);
                if (result != null) return result;
            }
        }

        return null;
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

    private async Task LoginToMundaManagerAsync(IPage page, string username, string password)
    {
        try
        {
            // Navigate to login page
            await page.GoToAsync("https://www.mundamanager.com/login", WaitUntilNavigation.Networkidle2);

            // Wait for login form to appear
            await page.WaitForSelectorAsync("input[type='email'], input[name='email'], input[placeholder*='email' i]", new WaitForSelectorOptions { Timeout = 5000 });

            // Fill in email/username
            var emailInput = await page.QuerySelectorAsync("input[type='email'], input[name='email'], input[placeholder*='email' i]");
            if (emailInput != null)
            {
                await emailInput.TypeAsync(username);
            }

            // Fill in password
            var passwordInput = await page.QuerySelectorAsync("input[type='password'], input[name='password']");
            if (passwordInput != null)
            {
                await passwordInput.TypeAsync(password);
            }

            // Submit the form using requestSubmit (React form requirement)
            var form = await page.QuerySelectorAsync("form");
            if (form != null)
            {
                _logger.LogInformation("Submitting login form...");
                await form.EvaluateFunctionAsync("form => form.requestSubmit()");

                // Wait for navigation
                try
                {
                    await page.WaitForNavigationAsync(new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle2 }, Timeout = 10000 });
                }
                catch (WaitTaskTimeoutException)
                {
                    _logger.LogWarning("Form submission may be blocked by CAPTCHA");
                }
            }

            _logger.LogInformation("Login attempt completed");
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Error logging into Munda Manager: {Error}", ex.Message);
            throw new InvalidOperationException("Failed to log into Munda Manager. Check credentials.", ex);
        }
    }

    /// <summary>
    /// Generates sample gang data for testing. Use this while PoC'ing authentication with Munda Manager.
    /// </summary>
    public (bool Success, Gang? Gang, string ErrorMessage) GenerateSampleGang()
    {
        var fighters = new List<MundaFighter>
        {
            new MundaFighter
            {
                Id = "sample-1",
                FighterName = "Boss Hank",
                FighterType = "Leader",
                FighterClass = "Gang Leader",
                Credits = 150,
                Movement = 4, WeaponSkill = 4, BallisticSkill = 3, Strength = 4, Toughness = 3,
                Wounds = 1, Initiative = 3, Attacks = 2, Leadership = 9, Cool = 7, Willpower = 6, Intelligence = 5,
                Weapons = new() { new() { Name = "Bolt Pistol", Type = "ranged" }, new() { Name = "Sword", Type = "melee" } },
                Wargear = new(),
                Effects = new() { Active = new() }
            },
            new MundaFighter
            {
                Id = "sample-2",
                FighterName = "Scarface",
                FighterType = "Ganger",
                FighterClass = "Juve",
                Credits = 75,
                Movement = 4, WeaponSkill = 3, BallisticSkill = 3, Strength = 3, Toughness = 3,
                Wounds = 1, Initiative = 3, Attacks = 1, Leadership = 6, Cool = 5, Willpower = 4, Intelligence = 4,
                Weapons = new() { new() { Name = "Lasgun", Type = "ranged" } },
                Wargear = new(),
                Effects = new() { Active = new() }
            }
        };

        var rawJson = System.Text.Json.JsonSerializer.Serialize(fighters);
        var gang = new Gang
        {
            Name = "Sample Gang",
            House = "Goliath",
            RawData = rawJson,
            MmSource = "Sample Data (for testing)",
            ImportedAt = DateTime.UtcNow
        };

        return (true, gang, "");
    }
}
