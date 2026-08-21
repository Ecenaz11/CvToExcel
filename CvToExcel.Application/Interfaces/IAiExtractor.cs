using CvToExcel.Application.Contracts;

namespace CvToExcel.Application.Interfaces;

public interface IAiExtractor
{
    Task<CvProcessingResult> ProcessCvAsync(Stream pdfStream, 
    string contentType, 
    IReadOnlyList<CvExtractionResult> existingCandidates,
    CancellationToken cancellationToken = default);
}