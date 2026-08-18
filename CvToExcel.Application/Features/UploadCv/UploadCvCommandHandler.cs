using MediatR;
using CvToExcel.Application.Interfaces;

namespace CvToExcel.Application.Features.UploadCv;

public class UploadCvCommandHandler(
    IFileStorage fileStorage,
    IAiExtractor aiExtractor) : IRequestHandler<UploadCvCommand, UploadCvResult>
{
    public async Task<UploadCvResult> Handle(UploadCvCommand request,
    CancellationToken cancellationToken)
    {
        var (storedFileName, filePath) = await fileStorage.SaveAsync(
            request.FileStream, request.OriginalFileName, cancellationToken);

        using var pdfStream = File.OpenRead(filePath);
        var rawAiResponse = await aiExtractor.ExtractCvDataAsync(pdfStream, request.ContentType, cancellationToken);

        return new UploadCvResult(storedFileName, filePath, rawAiResponse);
    }
}
