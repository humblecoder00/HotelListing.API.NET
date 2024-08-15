namespace HotelListing.API.Models
{
    public class PagedResult<T>
    {
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int RecordNumber { get; set; } /* how many records are coming back, page size */
        public List<T> Items { get; set; }
    }
}
