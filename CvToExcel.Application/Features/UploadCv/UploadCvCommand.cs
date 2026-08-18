using MediatR;

namespace CvToExcel.Application.Features.UploadCv;

public record UploadCvCommand(
    Stream FileStream,
    string OriginalFileName,
    string ContentType) : IRequest<UploadCvResult>;

    public record UploadCvResult(
        string StoredFileName,
        string FilePath,
        string RawAiResponse
    );
