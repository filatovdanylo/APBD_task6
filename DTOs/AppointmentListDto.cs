namespace APBD_TASK6.DTOs
{
    public class AppointmentListDto
    {
        public int IdAppointment { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string PatientFullName { get; set; } = string.Empty;
        public string PatientEmail { get; set; } = string.Empty;

        public AppointmentListDto(int idAppointment, DateTime appointmentDate, string status, string reason, string patientFullName, string patientEmail)
        {
            IdAppointment = idAppointment;
            AppointmentDate = appointmentDate;
            Status = status;
            Reason = reason;
            PatientFullName = patientFullName;
            PatientEmail = patientEmail;
        }
        public AppointmentListDto() { }
    }
}
