using FileHandlerApplication.Interfaces;
using FileHandlerApplication.Models;
using FileHandlerApplication.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileHandlerApplication.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FileController(IFileService fileService)
        {
            _fileService = fileService;
        }

        [HttpGet("get-by-name/{name}")]
        public async Task<ActionResult<byte[]?>> GetFile(string name)
        {
            try
            {
                var result = await _fileService.GetFileData(name);
                if (result != null)
                {
                    return Ok(result);
                }
                return BadRequest("File not found");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

        }

        [Authorize(Roles = "HR")]
        [HttpPost("upload")]
        public async Task<ActionResult<FileData>> PostFile([FromForm] FileUploadDto dto)
        {
            try
            {
                if (dto.UploadFile == null || dto.UploadFile.Length == 0)
                {
                    return BadRequest("File is empty");
                }
                var result = await _fileService.UploadFile(dto.UploadFile);
                if (result != null)
                {
                    return Created("", result);
                }
                return BadRequest("Unable to create the file at the moment.");
            }
            catch (Exception e)
            {
                return Unauthorized(e.Message);
            }

        }

    }
}