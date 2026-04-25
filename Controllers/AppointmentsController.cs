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
        public async Task<ActionResult<List<AppointmentListDto>>> GetAppointments()
        {
            var appointments = await _appointmentsService.GetAllAppointmentsAsync();

            if (appointments == null || appointments.Count == 0)
            {
                return NotFound();
            }

            return Ok(appointments);
        }

    }
}
