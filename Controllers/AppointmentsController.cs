using APBD_TASK6.DTOs;
using APBD_TASK6.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<ActionResult<List<AppointmentListDto>>> GetAppointments(
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

    }
}
