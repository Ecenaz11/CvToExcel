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

        var command = new UploadCvCommand(stream, file.FileName, file.ContentType);
        var result = await sender.Send(command, cancellationToken);

        return Ok(result);
    }
}