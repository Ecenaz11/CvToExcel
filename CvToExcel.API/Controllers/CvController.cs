using CvToExcel.Application.Contracts;
using CvToExcel.Application.Features.GetCv;
using CvToExcel.Application.Features.UploadCv;
using CvToExcel.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CvToExcel.API.Controllers;

[ApiController]
[Route("api/cv")]
public class CvController(ISender sender, IExcelWriter excelWriter) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<UploadCvResult>> UploadCv(IFormFile file, CancellationToken cancellationToken)
    {
        if(file.Length==0)
        {
            return BadRequest("Dosya boş olamaz.");
        }
        await using var stream = file.OpenReadStream();

        var command = new UploadCvCommand(stream, file.FileName, file.ContentType, file.Length);
        var result = await sender.Send(command, cancellationToken);

        return Ok(result);
    }
    [HttpGet]
    public async Task<ActionResult<List<CvSummaryResult>>> GetCv([FromQuery] Guid? id, CancellationToken cancellationToken)
    {
        var query = new GetCvQuery(id);
        var result = await sender.Send(query, cancellationToken);
        return Ok(result);
    }
    [HttpGet("excel")]
    public IActionResult DownloadExcel()
    {
        var filePath = excelWriter.GetFilePath();
        if(System.IO.File.Exists(filePath))
        {
            return PhysicalFile(filePath, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "candidates.xlsx");
        }
        return NotFound("Henüz hiçbir CV dosyası yüklenmediği için Excel dosyası oluşturulamadı.");
    }
}