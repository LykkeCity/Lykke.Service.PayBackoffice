using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackOffice.Models
{
    public class PagedListModel
    {
        public int PageSize { get; set; }
        public int Count { get; set; }
        public int CurrentPage { get; set; }
        public string Action { get; set; }
        public string FormId { get; set; }
        public string DivResult { get; set; }
    }
}
