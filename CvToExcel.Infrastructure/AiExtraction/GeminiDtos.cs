using System.Text.Json.Serialization;

namespace CvToExcel.Infrastructure.AiExtraction;

public class GeminiGenerateContentRequest
{
    public List<GeminiContent> Contents { get; set; } = new();
    public GeminiGenerationConfig? GenerationConfig { get; set; }
}
public class GeminiContent
{
    public List<GeminiPart> Parts { get; set; } = new();
}
public class GeminiPart
{
    public string? Text { get; set; }

    [JsonPropertyName("inline_data")]
    public GeminiInlineData? InlineData { get; set; }
}

public class GeminiInlineData
{
    [JsonPropertyName("mime_type")]
    public string MimeType { get; set; } = string.Empty;

    public string Data { get; set; } = string.Empty;
}

public class GeminiGenerationConfig
{
    [JsonPropertyName("response_mime_type")]
    public string ResponseMimeType { get; set; } = "application/json";
}

public class GeminiGenerateContentResponse
{
    public List<GeminiCandidate> Candidates { get; set; } = new();
}

public class GeminiCandidate
{
    public GeminiContent Content { get; set; } = new();
}