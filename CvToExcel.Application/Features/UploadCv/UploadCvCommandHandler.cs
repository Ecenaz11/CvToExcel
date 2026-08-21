using MediatR;
using CvToExcel.Application.Interfaces;
using FluentValidation;
using CvToExcel.Application.Contracts;
using System.IO.Pipelines;
using CvToExcel.Application.Exceptions;

namespace CvToExcel.Application.Features.UploadCv;

public class UploadCvCommandHandler(
    IFileStorage fileStorage,
    IAiExtractor aiExtractor,
    IValidator<CvExtractionResult> validator,
    ICvDocumentRepository repository,
    IExcelWriter excelWriter) : IRequestHandler<UploadCvCommand, UploadCvResult>
{
    public async Task<UploadCvResult> Handle(UploadCvCommand request,
    CancellationToken cancellationToken)
    {
        var existingDocuments = await repository.GetAllAsync(cancellationToken);
        var existingCandidates = existingDocuments.Select(CvDocumentMapper.ToDto).ToList();
        var (storedFileName, filePath) = await fileStorage.SaveAsync(
            request.FileStream, request.OriginalFileName, cancellationToken);

        using var pdfStream = File.OpenRead(filePath);
        var result = await aiExtractor.ProcessCvAsync(pdfStream, request.ContentType, existingCandidates, cancellationToken);

        await validator.ValidateAndThrowAsync(result.NewCandidate, cancellationToken);

         if(existingCandidates.Any(c => c.Email is not null && c.Email == result.NewCandidate.Email))
        {
            throw new DuplicateCandidateException($"Bu email adresine sahip bir aday zaten sistemde kayıtlı: {result.NewCandidate.Email}");
        }
        var cvDocument = CvDocumentMapper.ToEntity(
            result.NewCandidate, request.OriginalFileName, storedFileName, 
            filePath, request.FileSize, request.ContentType);

        await repository.AddAsync(cvDocument,cancellationToken);
        await excelWriter.WriteAsync(result.Table, cancellationToken);
        
        return new UploadCvResult(storedFileName, filePath, result);
    }
}
