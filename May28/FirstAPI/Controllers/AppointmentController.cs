using FirstAPI.Interfaces;
using FirstAPI.Models;
using FirstAPI.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FirstAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpPost]
        public async Task<ActionResult<Appointment>> PostAppointment([FromBody] AppointmentAddRequestDto dto)
        {
            try
            {
                var appointment = await _appointmentService.MakeAppointment(dto);
                if (appointment == null)
                {
                    return BadRequest("Unable to create an appointment at the moment");
                }
                return Created("", appointment);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }


        [Authorize(Policy = "ExperiencedDoctorOnly")]
        [HttpPut("cancel_appointment/{appointment_id}")]
        public async Task<ActionResult<Appointment>> PutCancelAppointment(int appointment_id)
        {

            var appointment = await _appointmentService.CancelAppointment(appointment_id);
            if (appointment == null)
            {
                return BadRequest("Cancellation failed.");
            }
            return Ok("Appointment has been cancelled");
        }

    }
}