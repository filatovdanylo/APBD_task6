using APBD_TASK6.DTOs;

namespace APBD_TASK6.Interfaces.Services
{
    public interface IAppointmentService
    {
        Task<List<AppointmentListDto>> GetAllAppointmentsAsync(string? status, string? patientLastName);
        Task<AppointmentDetailsDto?> GetAppointmentByIdAsync(int id);
        Task<int> CreateAppointmentAsync();
        Task<bool> UpdateAppointmentAsync();
    }
}
