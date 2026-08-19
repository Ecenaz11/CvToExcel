using CvToExcel.Domain.Entities;

namespace CvToExcel.Application.Interfaces;

public interface ICvDocumentRepository
{
    Task AddAsync(CvDocument cvDocument, CancellationToken cancellationToken = default);
}