using FluentValidation;
using CvToExcel.Application.Contracts;

namespace CvToExcel.Application.Validators;

public class CvExtractionResultValidator : AbstractValidator<CvExtractionResult>
{
    private static readonly string[] AllowedskillTypes = {"Technical", "Soft", "Language", "Tool"};
    public CvExtractionResultValidator()
    {
        RuleForEach(x => x.Skills).ChildRules(skill =>
        {
            skill.RuleFor(s => s.SkillType)
            .Must(type => AllowedskillTypes.Contains(type))
            .WithMessage(s => $"Geçersiz skillType: '{s.SkillType}'. İzin verilen değerler : Technical, Soft, Language, Tool.");
        });
    }
}
