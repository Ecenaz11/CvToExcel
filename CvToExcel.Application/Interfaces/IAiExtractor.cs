using CvToExcel.Application.Contracts;

namespace CvToExcel.Application.Interfaces;

public interface IAiExtractor
{
    Task<CvExtractionResult> ExtractCvDataAsync(Stream pdfStream, string contentType,
    CancellationToken cancellationToken = default);
}