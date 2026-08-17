namespace Picterest.HelperModels
{
    public class PaginatedResponse<T>
    {
        public IReadOnlyList<T> Items { get; set; } = new List<T>();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }


        public int TotalRecordCount { get; set; }
        public int TotalPageCount { get; set; }

        public bool HasNextPage => PageNumber < TotalPageCount;
        public bool HasPreviousPage => PageNumber > 1;
    }
}
