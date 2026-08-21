namespace CvToExcel.Application.Contracts;

public class CvExtractionResult
{
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Location { get; set; }
    public List<EducationDto> Educations { get; set; } = new();
    public List<WorkExperienceDto> WorkExperiences { get; set; } = new();
    public List<SkillDto> Skills { get; set; } = new();
    public List<LanguageDto> Languages { get; set; } = new();
    public List<ProjectDto> Projects {get;set;} = new();
    public List <OtherSectionDto> OtherSections {get;set;} = new();
}

public class EducationDto
{
    public string Institution { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? Degree { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
}
public class WorkExperienceDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
}
public class SkillDto
{
    public string Name { get; set; } = string.Empty;
    public string SkillType { get; set; } = string.Empty;
}

public class LanguageDto
{
    public string Name { get; set; } = string.Empty;
    public string ProficiencyLevel { get; set; } = string.Empty;
}

public class OtherSectionDto
{
    public string Title{get; set;} = string.Empty;
    public string? Content {get;set;} 
}

public class ProjectDto
{
    public string Title{get;set;} = string.Empty;
    public string? TechnologiesUsed{get;set;}
    public string? Description{get;set;}
}