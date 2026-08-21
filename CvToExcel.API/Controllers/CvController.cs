using CvToExcel.Application.Contracts;
using CvToExcel.Application.Features.GetCvStatus;
using CvToExcel.Application.Features.UploadCv;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CvToExcel.API.Controllers;

[ApiController]
[Route("api/cv")]
public class CvController(ISender sender) : ControllerBase
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
    public async Task<ActionResult<List<CvStatusResult>>> GetCvStatus([FromQuery] Guid? id, CancellationToken cancellationToken)
    {
        var query = new GetCvStatusQuery(id);
        var result = await sender.Send(query, cancellationToken);
        return Ok(result);
    }
}