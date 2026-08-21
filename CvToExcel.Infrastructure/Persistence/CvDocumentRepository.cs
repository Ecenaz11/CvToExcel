using CvToExcel.Application.Interfaces;
using CvToExcel.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CvToExcel.Infrastructure.Persistence;

public class CvDocumentRepository(AppDbContext context) : ICvDocumentRepository
{
    public async Task AddAsync(CvDocument cvDocument, CancellationToken cancellationToken = default)
    {
        context.CvDocuments.Add(cvDocument);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<CvDocument>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.CvDocuments
        .Include(c => c.Educations)
        .Include(c => c.WorkExperiences)
        .Include(c => c.Skills)
        .Include(c => c.Languages)
        .Include(c => c.Projects)
        .Include(c => c.OtherSections)
        .ToListAsync(cancellationToken);
    }

    public async Task<CvDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.CvDocuments.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }
}