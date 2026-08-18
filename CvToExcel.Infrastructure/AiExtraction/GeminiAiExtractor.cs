using System.Text;
using System.Text.Json;
using CvToExcel.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace CvToExcel.Infrastructure.AiExtraction;

public class GeminiAiExtractor(HttpClient httpClient,
 IOptions<GeminiOptions> options) : IAiExtractor
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private const string Prompt = """
        Bu bir CV (özgeçmiş) PDF dosyasıdır. İçeriğini analiz et ve aşağıdaki JSON şemasına birebir uyan bir JSON döndür:

        {
          "fullName": "string",
          "email": "string veya null",
          "phone": "string veya null",
          "location": "string veya null",
          "educations": [
            { "institution": "string", "department": "string veya null", "degree": "string veya null", "startDate": "YYYY-MM-DD veya null", "endDate": "YYYY-MM-DD veya null", "description": "string veya null" }
          ],
          "workExperiences": [
            { "companyName": "string", "jobTitle": "string", "startDate": "YYYY-MM-DD veya null", "endDate": "YYYY-MM-DD veya null", "description": "string veya null" }
          ],
          "skills": [
            { "name": "string", "skillType": "Technical | Soft | Language | Tool" }
          ],
          "languages": [
            { "name": "string", "proficiencyLevel": "string" }
          ]
        }

        Kurallar:
        - workExperiences[].description alanını CV metninden birebir al, kendi yorumunu/özetini katma. CV'de bu iş için açıklama yoksa null bırak.
        - CV'de olmayan alanlar için null kullan, alanı uydurma.
        - skillType sadece şu dört değerden biri olmalı: Technical, Soft, Language, Tool.
        - Yanıtın SADECE JSON olsun, başka açıklama/metin ekleme.
        """;

    public async Task<string> ExtractCvDataAsync(Stream pdfStream, string contentType, CancellationToken cancellationToken = default)
    {
        using var memoryStream = new MemoryStream();
        await pdfStream.CopyToAsync(memoryStream, cancellationToken);
        var base64Pdf = Convert.ToBase64String(memoryStream.ToArray());

        var request = new GeminiGenerateContentRequest
        {
            Contents = new List<GeminiContent>
            {
                new()
                {
                    Parts = new List<GeminiPart>
                    {
                        new() { Text = Prompt },
                        new() { InlineData = new GeminiInlineData { MimeType = contentType, Data = base64Pdf } }
                    }
                }
            },
            GenerationConfig = new GeminiGenerationConfig()
        };

        var geminiOptions = options.Value;
        var requestUri = $"https://generativelanguage.googleapis.com/v1beta/models/{geminiOptions.Model}:generateContent";

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUri);
        httpRequest.Headers.Add("x-goog-api-key", geminiOptions.ApiKey);
        httpRequest.Content = new StringContent(
            JsonSerializer.Serialize(request, JsonOptions),
            Encoding.UTF8,
            "application/json");

        using var httpResponse = await httpClient.SendAsync(httpRequest, cancellationToken);
        var responseBody = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Gemini API error {(int)httpResponse.StatusCode}: {responseBody}");
        }

        var geminiResponse = JsonSerializer.Deserialize<GeminiGenerateContentResponse>(responseBody, JsonOptions);

        return geminiResponse?.Candidates.FirstOrDefault()?.Content.Parts.FirstOrDefault()?.Text
            ?? throw new InvalidOperationException("Gemini API'den beklenen formatta bir cevap alınamadı.");
    }
}
