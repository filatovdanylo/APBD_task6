using APBD_TASK6.DTOs;
using APBD_TASK6.Interfaces.Services;
using Microsoft.Data.SqlClient;
using System.Net.NetworkInformation;
using System.Runtime.ConstrainedExecution;

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

        public Task<int> CreateAppointmentAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<List<AppointmentListDto>> GetAllAppointmentsAsync(string status, string patientLastName)
        {
            var appointments = new List<AppointmentListDto>();
            
            await using var connection = new SqlConnection(_connectionString);
            string query = @"
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

        public async Task<AppointmentDetailsDto?> GetAppointmentByIdAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateAppointmentAsync()
        {
            throw new NotImplementedException();
        }


        private AppointmentListDto MapToListAppointment(SqlDataReader reader)
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
    }
}
