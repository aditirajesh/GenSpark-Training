using Microsoft.AspNetCore.Mvc;
using FirstAPI.Models;
using FirstAPI.Models.DTOs;
using FirstAPI.Interfaces;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("/api/[controller]")]

public class PatientController : ControllerBase 
{
    private readonly IPatientService _patientService;

    public PatientController(IPatientService patientService) {
        _patientService = patientService;
    }

    [HttpGet("get-by-name/{name}")]
    [Authorize]
    public async Task<ActionResult<Patient>> GetPatient(string name) {
        try
        {
            var newPatient = await _patientService.GetPatientByName(name);
            if (newPatient != null)
            {
                return Ok(newPatient);
            }
            return BadRequest("Unable to find the user");
        }
        catch (Exception e)
        {
            return Unauthorized(e.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<Patient>> PostPatient([FromBody] PatientAddRequestDto dto) {
        try
        {
            var newPatient = await _patientService.AddPatient(dto);
            if (newPatient != null)
            {
                return Created("", newPatient);
            }
            return BadRequest("Unable to create patient at the moment");
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

}