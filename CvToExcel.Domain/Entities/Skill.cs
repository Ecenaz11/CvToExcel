using CvToExcel.Domain.Enums;

namespace CvToExcel.Domain.Entities;

public class Skill
{
    public Guid Id {get; set;}
    public Guid CvDocumentId{get; set;}
    public CvDocument CvDocument { get; set; } = null!;
    public required string Name {get; set;}
    public SkillType SkillType {get; set;}
}