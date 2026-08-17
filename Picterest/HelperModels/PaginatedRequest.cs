namespace Picterest.HelperModels
{
    public class PaginatedRequest
    {
        private const int MaxPageSize = 100;
        public int PageNumber { get; set; } = 1;
        private int _pageSize = 10;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = Math.Clamp(value,1,MaxPageSize);
        }
        public string search { get; set; } = string.Empty;
        public SortFilters SortFilters { get; set; } = new SortFilters();

    }
    public class SortFilters
    {
        public string Name { get; set; } = "ASC";
        public string Description { get; set; } = "ASC";
        public string Size { get; set; } = "ASC";
        public string CreatedAt { get; set; } = "ASC";

    }
}   
