namespace APBD_TASK6.DTOs
{
    public class AppointmentDetailsDto
    {
        public int IdAppointment { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; }
        public string Reason { get; set; }
        public string InternalNotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public int IdPatient { get; set; }
        public string PatientFullName { get; set; }
        public string PatientEmail { get; set; }
        public string PatientPhoneNumber { get; set; }
        public DateTime PatientDateOfBirth { get; set; }
        public int IdDoctor { get; set; }
        public string DoctorFullName { get; set; }
        public string DoctorSpecialization { get; set; }
        public string DoctorLicenseNumber { get; set; }
        public AppointmentDetailsDto(int idAppointment, DateTime appointmentDate, string status, string reason, string internalNotes, DateTime createdAt, int idPatient, string patientFullName, string patientEmail, string patientPhoneNumber, DateTime patientDateOfBirth, int idDoctor, string doctorFullName, string doctorSpecialization, string doctorLicenseNumber)
        {
            IdAppointment = idAppointment;
            AppointmentDate = appointmentDate;
            Status = status;
            Reason = reason;
            InternalNotes = internalNotes;
            CreatedAt = createdAt;
            IdPatient = idPatient;
            PatientFullName = patientFullName;
            PatientEmail = patientEmail;
            PatientPhoneNumber = patientPhoneNumber;
            PatientDateOfBirth = patientDateOfBirth;
            IdDoctor = idDoctor;
            DoctorFullName = doctorFullName;
            DoctorSpecialization = doctorSpecialization;
            DoctorLicenseNumber = doctorLicenseNumber;
        }

        public AppointmentDetailsDto() { }
    }
}
