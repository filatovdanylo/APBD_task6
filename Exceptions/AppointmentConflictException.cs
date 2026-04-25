namespace APBD_TASK6.Exceptions
{
    public class AppointmentConflictException : Exception
    {

        public AppointmentConflictException() { }
        public AppointmentConflictException(string message) : base(message) { }
    }
}
