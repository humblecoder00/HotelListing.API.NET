namespace HotelListing.API.Exceptions
{
    // ApplicationException is a base class where all application-specific exceptions derive from
    public class NotFoundException : ApplicationException
    {
        // We pass "name" to the base class constructor (ApplicationException) to set the message
        public NotFoundException(string name, object key) : base($"{name} ({key}) was not found")
        {
            
        }
    }
}
