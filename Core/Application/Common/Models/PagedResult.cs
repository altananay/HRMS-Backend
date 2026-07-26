namespace Application.Common.Models
{
    public sealed record PagedResult<T>(
        IReadOnlyList<T> Items,
        int Page,
        int PageSize,
        int TotalCount)
    {
        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

        public bool HasPreviousPage => Page > 1;

        public bool HasNextPage => Page < TotalPages;

        public static PagedResult<T> Empty(PageRequest request)
            => new([], request.Page, request.PageSize, 0);
    }

    public sealed record PageRequest
    {
        public const int MaxPageSize = 100;
        public const int DefaultPageSize = 20;

        public PageRequest() { }

        public PageRequest(int page, int pageSize)
        {
            Page = page;
            PageSize = pageSize;
        }

        private readonly int _page = 1;
        private readonly int _pageSize = DefaultPageSize;

        public int Page
        {
            get => _page;
            init => _page = value < 1 ? 1 : value;
        }

        public int PageSize
        {
            get => _pageSize;
            init => _pageSize = value switch
            {
                < 1 => DefaultPageSize,
                > MaxPageSize => MaxPageSize,
                _ => value
            };
        }

        public int Skip => (Page - 1) * PageSize;
    }
}
