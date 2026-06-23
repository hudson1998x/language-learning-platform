using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using LLE.Kernel.Attributes;
using LLE.Kernel.Registry;
using LLE.Kernel.Security;
using LLE.LLMFramework.Models;
using LLE.LLMFramework.Services;
using LLE.MusicTranslation.Dto;
using LLE.MusicTranslation.Media.Albums;
using LLE.MusicTranslation.Media.Artists;
using LLE.MusicTranslation.Media.Tracks;

namespace LLE.MusicTranslation.Services;

[Service]
public class MusicTranslationService(
    IArtistRepository artistRepo,
    IAlbumRepository albumRepo,
    ITrackRepository trackRepo)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private const int BatchSize = 8;
    private const int MaxTokensPerBatch = 1200;
    private const int MaxRetries = 2;

    private const string SystemPrompt = """
                                        You are a professional linguist and music translator.
                                        You specialise in phonetic transcription and cultural annotation of song lyrics.

                                        Rules you must never break:
                                          - Always respond with a raw JSON array. No markdown fences, no prose, nothing else.
                                          - Every object in the array MUST include a non-empty "pronunciation" field.
                                          - "pronunciation" must be human-readable and designed for learners to pronounce correctly.
                                          - NEVER use IPA symbols or phonetic alphabets.

                                        Pronunciation format rules:
                                          - Break pronunciation into small, speakable chunks separated by spaces.
                                          - Each chunk should represent a natural syllable or phonetic unit.
                                          - Use simple Latin letters only (a–z), plus optional diacritics only for tone where required.
                                          - Avoid academic transliteration styles unless they are already simple (e.g. pinyin, romaji).

                                        Language-specific rules:
                                          Chinese  -> Pinyin with tone marks, spaced into syllables
                                                      e.g. "wǒ ài nǐ" or "wo ai ni" (preferred: tone marks if available)

                                          Japanese -> Hepburn romaji, spaced by natural mora groups
                                                      e.g. "wa ta shi wa a na ta ga su ki de su"

                                          Korean   -> Revised Romanization, syllable-separated
                                                      e.g. "sa rang hae yo"

                                          Arabic   -> simple Latin pronunciation (not IPA, not academic transcription)
                                                      e.g. "a hib bu ka"

                                          Hindi    -> IAST or simplified readable Latin form
                                                      e.g. "tu jh se pyaar hai"

                                          English  -> respelled phonetic English (NOT IPA)
                                                      e.g. "ai lav yoo", "hu uh yu"

                                          Other    -> best-effort phonetic spelling in spaced syllables

                                        Hard constraints:
                                          - Do not use IPA symbols (/ /, ˈ, ʒ, ɪ, etc.)
                                          - Do not use dense transliteration systems that require linguistic knowledge
                                          - Prefer readability over academic correctness
                                          - pronunciation must always be present and non-empty
                                        """;

    public async Task<SongTranslationResponse> TranslateAsync(SongTranslationRequest request, UserContext ctx)
    {
        if (string.IsNullOrWhiteSpace(request.Title) ||
            string.IsNullOrWhiteSpace(request.Artist) ||
            string.IsNullOrWhiteSpace(request.Album))
        {
            throw new InvalidOperationException("Title, Artist, and Album are required");
        }

        var llmService = ServiceCatalog.GetService<LLMService>();

        var allLines = request.Lyrics
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var uniqueLines = allLines
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(line => !line.StartsWith("[") && !line.StartsWith("#"))
            .ToArray();

        var translations = new Dictionary<string, SongLine>(StringComparer.OrdinalIgnoreCase);

        // --- Pass 1: translate all lines in parallel batches ---
        var batches = uniqueLines
            .Select((line, i) => (line, i))
            .GroupBy(x => x.i / BatchSize)
            .Select(g => g.Select(x => x.line).ToArray())
            .ToArray();

        var batchTasks = batches.Select(async batch =>
        {
            var results = await TranslateBatchWithRetryAsync(
                llmService, batch, request.Artist, request.Title);

            lock (translations)
            {
                foreach (var (line, songLine) in results)
                    translations[line] = songLine;
            }
        });

        await Task.WhenAll(batchTasks);

        // --- Pass 2: retry lines where pronunciation is still missing ---
        var missingPronunciation = translations
            .Where(kv => kv.Value.Pronunciations is null || kv.Value.Pronunciations.Length == 0)
            .Select(kv => kv.Key)
            .ToArray();

        if (missingPronunciation.Length > 0)
        {
            var retryBatches = missingPronunciation
                .Select((line, i) => (line, i))
                .GroupBy(x => x.i / BatchSize)
                .Select(g => g.Select(x => x.line).ToArray());

            var retryTasks = retryBatches.Select(async batch =>
            {
                var results = await TranslatePronunciationOnlyAsync(
                    llmService, batch, request.Artist, request.Title);

                lock (translations)
                {
                    foreach (var (line, pronunciation) in results)
                    {
                        if (!translations.TryGetValue(line, out var existing) ||
                            string.IsNullOrWhiteSpace(pronunciation))
                            continue;

                        // SongLine is not a record — construct a new instance manually
                        translations[line] = new SongLine
                        {
                            LineContents = existing.LineContents,
                            TranslationToUserLanguage = existing.TranslationToUserLanguage,
                            CulturalMeaning = existing.CulturalMeaning,
                            Pronunciations = [pronunciation]
                        };
                    }
                }
            });

            await Task.WhenAll(retryTasks);
        }

        if (translations.Count == 0)
            throw new InvalidOperationException("Translation returned no lines");

        var orderedLines = allLines
            .Select(line => translations.TryGetValue(line, out var t) ? t : FallbackLine(line))
            .ToList();

        // --- Persistence ---
        var artist = await artistRepo.FindByNameAsync(request.Artist, ctx, DataOptions.Default)
                     ?? await artistRepo.CreateAsync(
                         new Artist { Name = request.Artist }, ctx, DataOptions.Default);

        var albumKey = $"{request.Artist}{request.Album}".ToLowerInvariant();
        var album = await albumRepo.FindByAlbumKeyAsync(albumKey, ctx, DataOptions.Default)
                    ?? await albumRepo.CreateAsync(
                         new Album { Title = request.Album, AlbumKey = albumKey },
                         ctx, DataOptions.Default);

        var track = await trackRepo.CreateAsync(new Track
        {
            Title = request.Title,
            ArtistId = artist.Id,
            AlbumId = album.Id,
            SongContents = JsonSerializer.Serialize(orderedLines, JsonOptions)
        }, ctx, DataOptions.Default);

        return new SongTranslationResponse { Song = track, Artist = artist, Album = album };
    }

    // -------------------------------------------------------------------------
    // Translation
    // -------------------------------------------------------------------------

    private static async Task<Dictionary<string, SongLine>> TranslateBatchWithRetryAsync(
        LLMService llmService,
        string[] lines,
        string artistName,
        string trackTitle)
    {
        Dictionary<string, SongLine>? result = null;

        for (var attempt = 0; attempt < MaxRetries; attempt++)
        {
            var raw = await QueryLlmAsync(
                llmService,
                SystemPrompt,
                BuildTranslationPrompt(lines, artistName, trackTitle),
                MaxTokensPerBatch);

            result = ParseBatchResponse(raw, lines);

            var allHavePronunciation = lines.All(l =>
                result.TryGetValue(l, out var s) &&
                s.Pronunciations is { Length: > 0 } &&
                !string.IsNullOrWhiteSpace(s.Pronunciations[0]));

            if (allHavePronunciation) break;
        }

        return result ?? lines.ToDictionary(l => l, FallbackLine, StringComparer.OrdinalIgnoreCase);
    }

    private static string BuildTranslationPrompt(string[] lines, string artist, string title)
    {
        // Schema is built as a plain string to avoid interpolation parser issues
        // with whitespace adjacent to format specifiers in raw string literals
        var schema =
            "{\n" +
            "  \"original\":        \"<lyric line, copied exactly>\",\n" +
            "  \"translation\":     \"<natural English translation>\",\n" +
            "  \"pronunciation\":   \"<phonetic romanisation — MANDATORY, never omit>\",\n" +
            "  \"culturalMeaning\": \"<1-2 sentences on cultural or idiomatic significance>\"\n" +
            "}";

        var example =
            "[\n" +
            "  {\n" +
            "    \"original\": \"我爱你\",\n" +
            "    \"translation\": \"I love you\",\n" +
            "    \"pronunciation\": \"Wo ai ni\",\n" +
            "    \"culturalMeaning\": \"A direct expression of romantic love, considered more intimate in Chinese culture than casual use.\"\n" +
            "  }\n" +
            "]";

        var numberedLines = string.Join("\n", lines.Select((l, i) => $"{i + 1}. {l}"));

        return
            $"Song: \"{title}\" by {artist}\n" +
            $"Number of lines: {lines.Length}\n\n" +
            $"Task: translate each lyric line and return a JSON array of exactly {lines.Length} objects.\n\n" +
            $"Required JSON schema for each object:\n{schema}\n\n" +
            "Romanisation guide (follow strictly):\n" +
            "  Chinese  -> Pinyin with tone marks  e.g. \"Wo ai ni\"\n" +
            "  Japanese -> Hepburn Romaji           e.g. \"Watashi wa anata ga suki desu\"\n" +
            "  Korean   -> Revised Romanization     e.g. \"Saranghae\"\n" +
            "  Arabic   -> ALA-LC                   e.g. \"Ahibbuka\"\n" +
            "  Hindi    -> IAST                     e.g. \"Tujhse pyar hai\"\n" +
            "  English  -> IPA                      e.g. \"/ai lav ju/\"\n" +
            "  Other    -> IPA\n\n" +
            $"Example output for the Chinese line \"我爱你\":\n{example}\n\n" +
            $"Now translate these {lines.Length} lines:\n{numberedLines}\n\n" +
            "JSON array:";
    }

    private static async Task<Dictionary<string, string>> TranslatePronunciationOnlyAsync(
        LLMService llmService,
        string[] lines,
        string artistName,
        string trackTitle)
    {
        var numberedLines = string.Join("\n", lines.Select((l, i) => $"{i + 1}. {l}"));

        var prompt =
            $"Song: \"{trackTitle}\" by {artistName}\n\n" +
            "For each lyric line below, provide ONLY the phonetic romanisation.\n" +
            "Use the correct system per language (Pinyin, Romaji, IPA, etc.).\n" +
            "Return ONLY a JSON array of objects with \"original\" and \"pronunciation\" keys.\n" +
            "No other fields. No markdown. No prose.\n\n" +
            $"Lines:\n{numberedLines}\n\n" +
            "JSON array:";

        var raw = await QueryLlmAsync(llmService, SystemPrompt, prompt, 512);
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        try
        {
            var json = StripMarkdownFences(raw);
            var array = JsonNode.Parse(json)?.AsArray();
            if (array is null) return result;

            foreach (var node in array)
            {
                var original = node?["original"]?.GetValue<string>() ?? string.Empty;
                var pronunciation = node?["pronunciation"]?.GetValue<string>() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(original) && !string.IsNullOrWhiteSpace(pronunciation))
                    result[original] = pronunciation;
            }
        }
        catch { /* best-effort */ }

        return result;
    }

    // -------------------------------------------------------------------------
    // Parsing
    // -------------------------------------------------------------------------

    private static Dictionary<string, SongLine> ParseBatchResponse(string raw, string[] originalLines)
    {
        var result = new Dictionary<string, SongLine>(StringComparer.OrdinalIgnoreCase);

        try
        {
            var json = StripMarkdownFences(raw);
            var array = JsonNode.Parse(json)?.AsArray()
                        ?? throw new JsonException("Response was not a JSON array");

            foreach (var node in array)
            {
                var original = node?["original"]?.GetValue<string>() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(original)) continue;

                var pronunciation = node?["pronunciation"]?.GetValue<string>()?.Trim() ?? string.Empty;

                result[original] = new SongLine
                {
                    LineContents = original,
                    TranslationToUserLanguage = node?["translation"]?.GetValue<string>()?.Trim() ?? string.Empty,
                    Pronunciations = string.IsNullOrWhiteSpace(pronunciation) ? [] : [pronunciation],
                    CulturalMeaning = node?["culturalMeaning"]?.GetValue<string>()?.Trim() ?? string.Empty
                };
            }
        }
        catch { /* fall through — ensure-all-present below handles it */ }

        foreach (var line in originalLines)
            result.TryAdd(line, FallbackLine(line));

        return result;
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static string StripMarkdownFences(string text)
    {
        var trimmed = text.Trim();
        if (!trimmed.StartsWith("```")) return trimmed;
        var firstNewline = trimmed.IndexOf('\n');
        var lastFence = trimmed.LastIndexOf("```");
        return firstNewline > 0 && lastFence > firstNewline
            ? trimmed[(firstNewline + 1)..lastFence].Trim()
            : trimmed;
    }

    private static SongLine FallbackLine(string line) => new()
    {
        LineContents = line,
        TranslationToUserLanguage = string.Empty,
        Pronunciations = [],
        CulturalMeaning = string.Empty
    };

    private static async Task<string> QueryLlmAsync(
        LLMService llmService,
        string systemPrompt,
        string userPrompt,
        int maxTokens)
    {
        try
        {
            var response = await llmService.SendMessageAsync("Ollama", userPrompt, req =>
            {
                req.MaxTokens = maxTokens;
                req.Instructions.Add(new Instruction { Content = systemPrompt });
            });
            return response.Content.Trim();
        }
        catch
        {
            return string.Empty;
        }
    }
}