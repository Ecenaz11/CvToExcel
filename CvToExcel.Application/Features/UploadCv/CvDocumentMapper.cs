using CvToExcel.Domain.Entities;
using CvToExcel.Domain.Enums;
using CvToExcel.Application.Contracts;

namespace CvToExcel.Application.Features.UploadCv;

public static class CvDocumentMapper
{
    public static CvDocument ToEntity(
        CvExtractionResult data, string originalFileName,
        string storedFileName, string filePath,
        long fileSize, string contentType)
    {
        return new CvDocument
        {
            FullName = data.FullName,
            Email = data.Email,
            Phone = data.Phone,
            Location = data.Location,
            OriginalFileName = originalFileName,
            StoredFileName = storedFileName,
            FilePath = filePath,
            FileSize = fileSize,
            ContentType = contentType,
            ProcessingStatus = ProcessingStatus.Completed,
            UploadAt = DateTime.UtcNow,
            ProcessedAt = DateTime.UtcNow,
            Educations = data.Educations.Select(e => new Education
            {
                Institution = e.Institution,
                Department = e.Department,
                Degree = e.Degree,
                StartDate = ParseDate(e.StartDate),
                EndDate = ParseDate(e.EndDate),
                Description = e.Description
            }).ToList(),
            WorkExperiences = data.WorkExperiences.Select(w => new WorkExperience
            {
                CompanyName = w.CompanyName,
                JobTitle = w.JobTitle,
                StartDate = ParseDate(w.StartDate),
                EndDate = ParseDate(w.EndDate),
                Description = w.Description
            }).ToList(),
            Skills = data.Skills.Select(s => new Skill
            {
                Name = s.Name,
                SkillType = Enum.Parse<SkillType>(s.SkillType)
            }).ToList(),
            Languages = data.Languages.Select(l => new Language
            {
                Name = l.Name,
                ProficiencyLevel = l.ProficiencyLevel
            }).ToList()
        };
    }
    private static DateTime? ParseDate(string? value) =>
    DateTime.TryParse(value, out var date) ? DateTime.SpecifyKind(date, DateTimeKind.Utc) : null;
    
}