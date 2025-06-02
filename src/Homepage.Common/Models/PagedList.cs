using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homepage.Common.Models
{
    public class PagedList<T> where T : class
    {
        public int PageIndex { get; set; } = 0;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
        public IEnumerable<T> Items { get; set; } = new List<T>();
    }
}
