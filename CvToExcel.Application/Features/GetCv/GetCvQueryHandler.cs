using CvToExcel.Application.Contracts;
using CvToExcel.Application.Features.UploadCv;
using CvToExcel.Application.Interfaces;
using MediatR;

namespace CvToExcel.Application.Features.GetCv;

public class GetCvQueryHandler(ICvDocumentRepository repository)
    : IRequestHandler<GetCvQuery, List<CvSummaryResult>>
{
    public async Task<List<CvSummaryResult>> Handle(GetCvQuery request, CancellationToken cancellationToken)
    {
        if(request.Id.HasValue)
        {
            var document = await repository.GetByIdAsync(request.Id.Value, cancellationToken);
            return document is null ? [] : [CvDocumentMapper.ToSummaryResult(document)];
        }
        var documents = await repository.GetAllAsync(cancellationToken);
        return documents.Select(CvDocumentMapper.ToSummaryResult).ToList();
    }
}
