using CvToExcel.Domain.Entities;

namespace CvToExcel.Application.Interfaces;

public interface ICvDocumentRepository
{
    Task AddAsync(CvDocument cvDocument,
     CancellationToken cancellationToken = default);

     Task<List<CvDocument>> GetAllAsync(CancellationToken cancellationToken = default);
     Task<CvDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}