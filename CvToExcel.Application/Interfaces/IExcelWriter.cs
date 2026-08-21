using CvToExcel.Application.Contracts;

namespace CvToExcel.Application.Interfaces;

public interface IExcelWriter
{
    Task WriteAsync(ExcelTableResult table, CancellationToken cancellationToken = default);
}