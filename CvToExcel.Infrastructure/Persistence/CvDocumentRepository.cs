using CvToExcel.Application.Interfaces;
using CvToExcel.Domain.Entities;

namespace CvToExcel.Infrastructure.Persistence;

public class CvDocumentRepository(AppDbContext context) : ICvDocumentRepository
{
    public async Task AddAsync(CvDocument cvDocument, CancellationToken cancellationToken = default)
    {
        context.CvDocuments.Add(cvDocument);
        await context.SaveChangesAsync(cancellationToken);
    }
}