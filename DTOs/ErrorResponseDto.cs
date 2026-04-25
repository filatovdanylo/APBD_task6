namespace APBD_TASK6.DTOs
{
    public class ErrorResponseDto
    {
        public string Error { get; set; } = string.Empty;
        public TimeOnly OccuredAt { get; set; }
    }
}
