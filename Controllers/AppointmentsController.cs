using APBD_TASK6.DTOs;
using APBD_TASK6.Exceptions;
using APBD_TASK6.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens.Experimental;

namespace APBD_TASK6.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentsService;

        public AppointmentsController(IAppointmentService appointmentsService)
        {
            _appointmentsService = appointmentsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAppointments(
            [FromQuery] string? status,
            [FromQuery] string? patientLastName)
        {
            var appointments = await _appointmentsService.GetAllAppointmentsAsync(status, patientLastName);

            if (appointments == null || appointments.Count == 0)
            {
                var error = new ErrorResponseDto
                {
                    Error = "Appointments with such query parameters do not exists",
                    OccuredAt = TimeOnly.FromDateTime(DateTime.Now)
                };
                return NotFound(error);
            }

            return Ok(appointments);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetAppointmentById(int id)
        {
            var appointment = await _appointmentsService.GetAppointmentByIdAsync(id);

            if (appointment == null)
            {
                var error = new ErrorResponseDto
                {
                    Error = $"Appointment with id {id} is not found",
                    OccuredAt = TimeOnly.FromDateTime(DateTime.Now)
                };
                return NotFound(error);
            }

            return Ok(appointment);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentRequestDto appointment)
        {
            try
            {
                int newID = await _appointmentsService.CreateAppointmentAsync(appointment);
                return CreatedAtAction(nameof(GetAppointmentById), new { id = newID }, appointment);
            }
            catch (InvalidOperationException ex)
            {
                var error = CreateErrorResponse(ex.Message);
                return NotFound(error);
            }
            catch (ArgumentException ex)
            {
                var error = CreateErrorResponse(ex.Message);
                return BadRequest(error);
            }
            catch (AppointmentConflictException ex)
            {
                var error = CreateErrorResponse(ex.Message);
                return Conflict(error);
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateAppointment(int id, [FromBody] UpdateAppointmentRequestDto appointment)
        {
            try
            {
                await _appointmentsService.UpdateAppointmentAsync(id, appointment);
                return Ok();
            } 
            catch (InvalidOperationException ex)
            {
                var error = CreateErrorResponse(ex.Message);
                return NotFound(error);
            } 
            catch (AppointmentConflictException ex)
            {
                var error = CreateErrorResponse(ex.Message);
                return Conflict(error);
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            try
            {
                int deleted = await _appointmentsService.DeleteAppointmentAsync(id);
                if (deleted == 0) 
                    return NotFound();
                return NoContent();
            } 
            catch (InvalidOperationException ex)
            {
                var error = CreateErrorResponse(ex.Message);
                return NotFound(error);
            }
            catch (AppointmentConflictException ex)
            {
                var error = CreateErrorResponse(ex.Message);
                return Conflict(error);
            }
        }

        private ErrorResponseDto CreateErrorResponse(string message)
        {
            return new ErrorResponseDto
            {
                Error = message,
                OccuredAt = TimeOnly.FromDateTime(DateTime.Now)
            };
        }

    }
}
