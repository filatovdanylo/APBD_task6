using APBD_TASK6.DTOs;
using APBD_TASK6.Exceptions;
using APBD_TASK6.Interfaces.Services;
using Microsoft.Data.SqlClient;

namespace APBD_TASK6.Services
{
    public class AppointmentService : IAppointmentService
    {

        private readonly string _connectionString;

        public AppointmentService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Missing 'DefaultConnection' in appsettings.json.");
        }

        public async Task<int> CreateAppointmentAsync(CreateAppointmentRequestDto appointment)
        {

            if (appointment.AppointmentDate <= DateTime.UtcNow)
                throw new ArgumentException("Appointment date cannot be in the past.");

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            bool doctorExists = await DoctorExistsAsync(appointment.IdDoctor, connection);
            if (!doctorExists)
            {
                throw new InvalidOperationException($"Doctor with id {appointment.IdDoctor} does not exist or is inactive");
            }

            bool patientExists = await PatientExistsAsync(appointment.IdPatient, connection);
            if (!patientExists)
            {
                throw new InvalidOperationException($"Patient with id {appointment.IdPatient} does not exists or is inactive");
            }


            bool isConflicting = await CheckConflictingAppointments(appointment.IdDoctor, appointment.AppointmentDate, connection);
            if (isConflicting)
            {
                throw new AppointmentConflictException($"Doctor already has an appointment at {appointment.AppointmentDate}");
            }
            


            const string insertQuery = """
                    INSERT INTO dbo.Appointments (IdPatient, IdDoctor, AppointmentDate, Reason, Status)
                    OUTPUT INSERTED.IdAppointment
                    VALUES (@IdPatient, @IdDoctor, @AppointmentDate, @Reason, 'Scheduled')
            """;

            await using var insertCmd = new SqlCommand(insertQuery, connection);
            insertCmd.Parameters.AddWithValue("@IdPatient", appointment.IdPatient);
            insertCmd.Parameters.AddWithValue("@IdDoctor", appointment.IdDoctor);
            insertCmd.Parameters.AddWithValue("@AppointmentDate", appointment.AppointmentDate);
            insertCmd.Parameters.AddWithValue("@Reason", appointment.Reason);

            var newId = await insertCmd.ExecuteScalarAsync();
            return (int) newId!;
        }

        public async Task<List<AppointmentListDto>> GetAllAppointmentsAsync(string? status, string? patientLastName)
        {
            var appointments = new List<AppointmentListDto>();
            
            await using var connection = new SqlConnection(_connectionString);
            const string query = @"
                SELECT 
                    a.IdAppointment, 
                    a.AppointmentDate, 
                    a.Status, 
                    a.Reason, 
                    p.FirstName + ' ' + p.LastName AS PatientFullName, 
                    p.Email AS PatientEmail 
                FROM dbo.Appointments a 
                JOIN dbo.Patients p ON p.IdPatient = a.IdPatient 
                WHERE (@Status IS NULL OR a.Status = @Status) 
                AND (@PatientLastName IS NULL OR p.LastName = @PatientLastName) 
                ORDER BY a.AppointmentDate;";
            var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@Status", status == null ? DBNull.Value : status);
            command.Parameters.AddWithValue("@PatientLastName", patientLastName == null ? DBNull.Value : patientLastName);

            await connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                appointments.Add(MapToListAppointment(reader));
            }

            return appointments;
        }

        public async Task<AppointmentDetailsDto?> GetAppointmentByIdAsync(int id)
        {
            await using var connection = new SqlConnection(_connectionString);
            const string query = """
                
                SELECT 
                    a.IdAppointment, 
                    a.AppointmentDate, 
                    a.Status, 
                    a.Reason, 
                    a.InternalNotes,
                    a.CreatedAt,
                    p.IdPatient,
                    p.FirstName + ' ' + p.LastName AS PatientFullName,
                    p.Email AS PatientEmail,
                    p.PhoneNumber AS PatientPhoneNumber,
                    p.DateOfBirth AS PatientDateOfBirth,
                    d.IdDoctor,
                    d.FirstName + ' ' + d.LastName AS DoctorFullName,
                    s.Name AS DoctorSpecialization,
                    d.LicenseNumber AS DoctorLicenseNumber
                FROM dbo.Appointments a 
                JOIN dbo.Patients p ON p.IdPatient = a.IdPatient 
                JOIN dbo.Doctors d ON d.IdDoctor = a.IdDoctor
                JOIN dbo.Specializations s ON d.IdSpecialization = s.IdSpecialization
                WHERE a.IdAppointment = @IdAppointment;
                
                """;

            var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@IdAppointment", id);

            await connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync();

            return await reader.ReadAsync() ? MapToAppointmentDetails(reader) : null;
        }

        public async Task UpdateAppointmentAsync(int id, UpdateAppointmentRequestDto appointment)
        {

            var databaseAppointment = await GetAppointmentByIdAsync(id);

            if (databaseAppointment == null)
            {
                throw new InvalidOperationException($"Appointment with id {id} does not exist");
            }

            if (databaseAppointment.Status == "Completed" 
                && appointment.AppointmentDate != databaseAppointment.AppointmentDate)
            {
                throw new AppointmentConflictException("Cannot change appointment date for Completed appointment");
            }

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();


            bool doctorExists = await DoctorExistsAsync(appointment.IdDoctor, connection);
            if (!doctorExists)
            {
                throw new InvalidOperationException($"Doctor with id {appointment.IdDoctor} does not exist or is inactive");
            }

            bool patientExists = await PatientExistsAsync(appointment.IdPatient, connection);
            if (!patientExists)
            {
                throw new InvalidOperationException($"Patient with id {appointment.IdPatient} does not exists or is inactive");
            }

            if (appointment.AppointmentDate != databaseAppointment.AppointmentDate)
            {
                bool isConflicting = await CheckConflictingAppointments(appointment.IdDoctor, appointment.AppointmentDate, connection);
                if (isConflicting)
                {
                    throw new AppointmentConflictException($"Doctor already has an appointment at {appointment.AppointmentDate}");
                }
            }


            const string updateQuery = """
                UPDATE dbo.Appointments
                SET IdPatient       = @IdPatient,
                    IdDoctor        = @IdDoctor,
                    AppointmentDate = @AppointmentDate,
                    Status          = @Status,
                    Reason          = @Reason,
                    InternalNotes   = @InternalNotes
                WHERE IdAppointment = @Id
            """;

            await using var updateCmd = new SqlCommand(updateQuery, connection);
            updateCmd.Parameters.AddWithValue("@Id", id);
            updateCmd.Parameters.AddWithValue("@IdPatient", appointment.IdPatient);
            updateCmd.Parameters.AddWithValue("@IdDoctor", appointment.IdDoctor);
            updateCmd.Parameters.AddWithValue("@AppointmentDate", appointment.AppointmentDate);
            updateCmd.Parameters.AddWithValue("@Status", appointment.Status);
            updateCmd.Parameters.AddWithValue("@Reason", appointment.Reason);
            updateCmd.Parameters.AddWithValue("@InternalNotes", (object?)appointment.InternalNotes ?? DBNull.Value);

            await updateCmd.ExecuteNonQueryAsync();

        }


        private static AppointmentListDto MapToListAppointment(SqlDataReader reader)
        {
            return new AppointmentListDto
            {
                IdAppointment = reader.GetInt32(reader.GetOrdinal("IdAppointment")),
                AppointmentDate = reader.GetDateTime(reader.GetOrdinal("AppointmentDate")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                Reason = reader.GetString(reader.GetOrdinal("Reason")),
                PatientFullName = reader.GetString(reader.GetOrdinal("PatientFullName")),
                PatientEmail = reader.GetString(reader.GetOrdinal("PatientEmail"))
            };
        }

        private static AppointmentDetailsDto MapToAppointmentDetails(SqlDataReader reader)
        {
            return new AppointmentDetailsDto
            {
                IdAppointment = reader.GetInt32(reader.GetOrdinal("IdAppointment")),
                AppointmentDate = reader.GetDateTime(reader.GetOrdinal("AppointmentDate")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                Reason = reader.GetString(reader.GetOrdinal("Reason")),
                InternalNotes = reader.IsDBNull(reader.GetOrdinal("InternalNotes"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("InternalNotes")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),

                IdPatient = reader.GetInt32(reader.GetOrdinal("IdPatient")),
                PatientFullName = reader.GetString(reader.GetOrdinal("PatientFullName")),
                PatientEmail = reader.GetString(reader.GetOrdinal("PatientEmail")),
                PatientPhoneNumber = reader.GetString(reader.GetOrdinal("PatientPhoneNumber")),
                PatientDateOfBirth = reader.GetDateTime(reader.GetOrdinal("PatientDateOfBirth")),

                IdDoctor = reader.GetInt32(reader.GetOrdinal("IdDoctor")),
                DoctorFullName = reader.GetString(reader.GetOrdinal("DoctorFullName")),
                DoctorSpecialization = reader.GetString(reader.GetOrdinal("DoctorSpecialization")),
                DoctorLicenseNumber = reader.GetString(reader.GetOrdinal("DoctorLicenseNumber"))
            };
        }

        private async Task<bool> DoctorExistsAsync(int doctorId, SqlConnection connection)
        {
            const string doctorQuery = "SELECT 1 FROM dbo.Doctors WHERE IdDoctor = @IdDoctor AND isActive = 1";
            await using (var doctorCmd = new SqlCommand(doctorQuery, connection))
            {
                doctorCmd.Parameters.AddWithValue("@IdDoctor", doctorId);
                var doctorExists = await doctorCmd.ExecuteScalarAsync();

                if (doctorExists is null)
                    return false;
            }

            return true;
        }

        private async Task<bool> PatientExistsAsync(int patientId, SqlConnection connection)
        {
            const string patientQuery = "SELECT 1 FROM dbo.Patients WHERE IdPatient = @IdPatient AND isActive = 1";
            await using (var patientCmd = new SqlCommand(patientQuery, connection))
            {
                patientCmd.Parameters.AddWithValue("@IdPatient", patientId);
                var patientExists = await patientCmd.ExecuteScalarAsync();

                if (patientExists is null)
                    return false;
            }

            return true;
        }

        private async Task<bool> CheckConflictingAppointments(int doctorId, DateTime appoinmentDate, SqlConnection connection)
        {
            const string conflictQuery = """
                    SELECT 1 FROM dbo.Appointments
                    WHERE IdDoctor = @IdDoctor
                    AND AppointmentDate = @AppointmentDate
                    AND Status != 'Cancelled'
            """;

            await using (var conflictCmd = new SqlCommand(conflictQuery, connection))
            {
                conflictCmd.Parameters.AddWithValue("@IdDoctor", doctorId);
                conflictCmd.Parameters.AddWithValue("@AppointmentDate", appoinmentDate);
                var conflict = await conflictCmd.ExecuteScalarAsync();

                if (conflict is not null)
                    return true;
            }

            return false;
        }
    }
}
