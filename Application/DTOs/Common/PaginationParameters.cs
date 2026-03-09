namespace DataLabelProject.Application.DTOs.Common
{
    public class PaginationParameters
    {
        private const int DefaultPage = 1;
        private const int DefaultPageSize = 10;
        private const int MaxPageSize = 100;
        private const int MinPageSize = 1;

        private int _page = DefaultPage;
        private int _pageSize = DefaultPageSize;

        public int Page
        {
            get => _page;
            set => _page = Math.Max(value, DefaultPage);
        }

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = Math.Clamp(value, MinPageSize, MaxPageSize);
        }

        public int Offset => (Page - 1) * PageSize;
    }
}