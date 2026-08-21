using CvToExcel.Application.Contracts;
using MediatR;

namespace CvToExcel.Application.Features.GetCv;

public record GetCvQuery(Guid? Id) : IRequest<List<CvSummaryResult>>;
