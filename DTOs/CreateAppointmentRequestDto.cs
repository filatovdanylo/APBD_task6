using System.ComponentModel.DataAnnotations;

namespace APBD_TASK6.DTOs
{
    public class CreateAppointmentRequestDto
    {
        public int IdPatient { get; set; }
        public int IdDoctor { get; set; }
        public DateTime AppointmentDate { get; set; }
        [Required]
        [StringLength(250, ErrorMessage = "Reason legnth cannot be more than 250")]
        public string Reason { get; set; } = string.Empty;
    }
}
