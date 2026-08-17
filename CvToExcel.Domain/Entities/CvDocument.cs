using CvToExcel.Domain.Enums;

namespace CvToExcel.Domain.Entities;

public class CvDocument
{
    public Guid Id {get; set;}
    public required string FullName {get;set;}
    public string? Email{get;set;}
    public string? Phone{get;set;}
    public string? Location {get;set;}
    public required string OriginalFileName{get;set;}
    public required string StoredFileName{get;set;}
    public required string FilePath{get;set;}
    public long FileSize{get;set;}
    public required string ContentType{get;set;}
    public ProcessingStatus ProcessingStatus{get;set;}
    public DateTime UploadAt{get;set;}
    public DateTime? ProcessedAt{get;set;}
    public ICollection<Education> Educations{get;set;} = new List<Education>();
    public ICollection<WorkExperience> WorkExperiences{get;set;} = new List<WorkExperience>();
    public ICollection<Skill> Skills{get;set;} = new List<Skill>();
    public ICollection<Language> Languages{get; set;} = new List<Language>();
}