using System.Collections.Generic;

namespace DataLabel_Project_BE.DTOs
{
    public class ProjectQueryParameters
    {
        private const int MaxPageSize = 100;
        private int _pageSize = 10;

        public int Page { get; set; } = 1;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
        }

        public string? Search { get; set; }

        /// <summary>
        /// Filter by a single category id
        /// </summary>
        public Guid? CategoryId { get; set; }

        /// <summary>
        /// Or filter by multiple category ids: ?categoryIds=guid1&amp;categoryIds=guid2
        /// </summary>
        public List<Guid>? CategoryIds { get; set; }

        public string? Status { get; set; }
    }
}