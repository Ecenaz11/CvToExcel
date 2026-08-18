using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace CvToExcel.Infrastructure.AiExtraction;

public class GeminiOptions
{
    public required string ApiKey{get;set;}
    public required string Model {get; set;}
}