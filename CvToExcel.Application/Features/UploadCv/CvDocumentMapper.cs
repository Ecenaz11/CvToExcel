using CvToExcel.Domain.Entities;
using CvToExcel.Domain.Enums;
using CvToExcel.Application.Contracts;
using System.Security.Cryptography.X509Certificates;

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
                IsCurrent = IsCurrentValue(e.EndDate)
            }).ToList(),
            WorkExperiences = data.WorkExperiences.Select(w => new WorkExperience
            {
                CompanyName = w.CompanyName,
                JobTitle = w.JobTitle,
                StartDate = ParseDate(w.StartDate),
                EndDate = ParseDate(w.EndDate),
                IsCurrent = IsCurrentValue(w.EndDate)
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
            }).ToList(),
            Projects = data.Projects.Select(p => new Project
            {
                Title = p.Title,
                TechnologiesUsed = p.TechnologiesUsed,
                Description = p.Description
            }).ToList(),
            OtherSections = data.OtherSections.Select(o => new OtherSection
            {
                Title = o.Title,
                Content = o.Content
            }).ToList() 
        };
    }
    public static CvExtractionResult ToDto (CvDocument cvDocument)
    {
        return new CvExtractionResult
        {
            FullName = cvDocument.FullName,
            Email = cvDocument.Email,
            Phone = cvDocument.Phone,
            Location = cvDocument.Location,
            Educations = cvDocument.Educations.Select(e => new EducationDto
            {
                Institution = e.Institution,
                Department = e.Department,
                Degree = e.Degree,
                StartDate = FormatDate(e.StartDate, false),
                EndDate = FormatDate(e.EndDate, e.IsCurrent)
            }).ToList(),
            WorkExperiences = cvDocument.WorkExperiences.Select(w => new WorkExperienceDto
            {
                CompanyName = w.CompanyName,
                JobTitle = w.JobTitle,
                StartDate = FormatDate(w.StartDate, false),
                EndDate = FormatDate(w.EndDate, w.IsCurrent)
            }).ToList(),
            Skills = cvDocument.Skills.Select( s => new SkillDto
            {
                Name = s.Name,
                SkillType = s.SkillType.ToString()
            }).ToList(),
            Languages = cvDocument.Languages.Select(l => new LanguageDto
            {
                Name = l.Name,
                ProficiencyLevel = l.ProficiencyLevel
            }).ToList(),
            Projects = cvDocument.Projects.Select(p => new ProjectDto
            {
                Title = p.Title,
                TechnologiesUsed = p.TechnologiesUsed,
                Description = p.Description
            }).ToList(),
            OtherSections = cvDocument.OtherSections.Select(o => new OtherSectionDto
            {
                Title = o.Title,
                Content = o.Content
            }).ToList()
        };
    }
    private static DateTime? ParseDate(string? value) =>
    DateTime.TryParse(value, out var date) ? DateTime.SpecifyKind(date, DateTimeKind.Utc) : null;
    private static bool IsCurrentValue(string? value) =>
    string.Equals(value?.Trim(), "current",
    StringComparison.OrdinalIgnoreCase);
    private static string? FormatDate(DateTime? date, bool isCurrent) =>
    isCurrent ? "current" : date?.ToString("yyyy-MM");

    public static CvStatusResult ToStatusResult(CvDocument cvDocument)
    {
        return new CvStatusResult
        {
        Id = cvDocument.Id,
        FullName = cvDocument.FullName,
        Email = cvDocument.Email,
        UploadAt = cvDocument.UploadAt
        };
    }
    
}