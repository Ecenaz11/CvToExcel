using CvToExcel.Application.Contracts;
using CvToExcel.Application.Features.UploadCv;
using CvToExcel.Application.Interfaces;
using MediatR;

namespace CvToExcel.Application.Features.GetCvStatus;

public class GetCvStatusQueryHandler(ICvDocumentRepository repository)
    : IRequestHandler<GetCvStatusQuery, List<CvStatusResult>>
{
    public async Task<List<CvStatusResult>> Handle(GetCvStatusQuery request, CancellationToken cancellationToken)
    {
        if(request.Id.HasValue)
        {
            var document = await repository.GetByIdAsync(request.Id.Value, cancellationToken);
            return document is null ? [] : [CvDocumentMapper.ToStatusResult(document)];
        }
        var documents = await repository.GetAllAsync(cancellationToken);
        return documents.Select(CvDocumentMapper.ToStatusResult).ToList();
    }
}