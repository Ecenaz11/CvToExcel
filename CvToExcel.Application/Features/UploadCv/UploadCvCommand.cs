using CvToExcel.Application.Contracts;
using MediatR;

namespace CvToExcel.Application.Features.UploadCv;

public record UploadCvCommand(
    Stream FileStream,
    string OriginalFileName,
    string ContentType,
    long FileSize) : IRequest<UploadCvResult>;

    public record UploadCvResult(
        string StoredFileName,
        string FilePath,
        CvExtractionResult CvData
    );
