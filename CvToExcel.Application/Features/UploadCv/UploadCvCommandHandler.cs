using MediatR;
using CvToExcel.Application.Interfaces;
using FluentValidation;
using CvToExcel.Application.Contracts;

namespace CvToExcel.Application.Features.UploadCv;

public class UploadCvCommandHandler(
    IFileStorage fileStorage,
    IAiExtractor aiExtractor,
    IValidator<CvExtractionResult> validator,
    ICvDocumentRepository repository) : IRequestHandler<UploadCvCommand, UploadCvResult>
{
    public async Task<UploadCvResult> Handle(UploadCvCommand request,
    CancellationToken cancellationToken)
    {
        var (storedFileName, filePath) = await fileStorage.SaveAsync(
            request.FileStream, request.OriginalFileName, cancellationToken);

        using var pdfStream = File.OpenRead(filePath);
        var cvData = await aiExtractor.ExtractCvDataAsync(pdfStream, request.ContentType, cancellationToken);

        await validator.ValidateAndThrowAsync(cvData, cancellationToken);

        var cvDocument = CvDocumentMapper.ToEntity(
            cvData, request.OriginalFileName, storedFileName, 
            filePath, request.FileSize, request.ContentType);

        await repository.AddAsync(cvDocument,cancellationToken);
        

        return new UploadCvResult(storedFileName, filePath, cvData);
    }
}
