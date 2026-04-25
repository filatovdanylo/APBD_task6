using System.ComponentModel.DataAnnotations;

namespace APBD_TASK6.DTOs
{
    public class UpdateAppointmentRequestDto
    {
        public int IdPatient { get; set; }
        public int IdDoctor { get; set; }
        public DateTime AppointmentDate { get; set; }
        [AllowedValues("Scheduled", "Completed", "Cancelled", ErrorMessage = "Status must be one of: Scheduled, Completed, Cancelled")]
        public string Status { get; set; } = string.Empty;
        [Required]
        [StringLength(250, ErrorMessage = "Reason legnth cannot be more than 250")]
        public string Reason {  get; set; } = string.Empty;
        public string? InternalNotes {  get; set; } = string.Empty;
    }
}
