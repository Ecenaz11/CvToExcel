namespace CvToExcel.Application.Interfaces;

public interface IAiExtractor
{
    Task<string> ExtractCvDataAsync(Stream pdfStream, string contentType,
    CancellationToken cancellationToken = default);
}