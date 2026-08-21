using CvToExcel.Application.Contracts;
using MediatR;

namespace CvToExcel.Application.Features.GetCvStatus;

public record GetCvStatusQuery(Guid? Id) : IRequest<List<CvStatusResult>>;