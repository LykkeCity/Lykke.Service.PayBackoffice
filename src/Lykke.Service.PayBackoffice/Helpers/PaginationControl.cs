using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOffice.Helpers
{
    public class PaginationControl
    {
        //public static string Paging(this HtmlHelper html, IPagedList pagedList, string url, string pagePlaceHolder)
        //{

        //    StringBuilder sb = new StringBuilder();

        //    // only show paging if we have more items than the page size
        //    if (pagedList.ItemCount > pagedList.PageSize)
        //    {

        //        sb.Append("<ul class=\"paging\">");

        //        if (pagedList.IsPreviousPage)
        //        { // previous link
        //            sb.Append("<li class=\"prev\"><a href=\"");
        //            sb.Append(url.Replace(pagePlaceHolder, pagedList.PageIndex.ToString()));
        //            sb.Append("\" title=\"Go to Previous Page\">prev</a></li>");
        //        }

        //        for (int i = 0; i < pagedList.PageCount; i++)
        //        {
        //            sb.Append("<li>");
        //            if (i == pagedList.PageIndex)
        //            {
        //                sb.Append("<span>").Append((i + 1).ToString()).Append("</span>");
        //            }
        //            else
        //            {
        //                sb.Append("<a href=\"");
        //                sb.Append(url.Replace(pagePlaceHolder, (i + 1).ToString()));
        //                sb.Append("\" title=\"Go to Page ").Append((i + 1).ToString());
        //                sb.Append("\">").Append((i + 1).ToString()).Append("</a>");
        //            }
        //            sb.Append("</li>");
        //        }

        //        if (pagedList.IsNextPage)
        //        { // next link
        //            sb.Append("<li class=\"next\"><a href=\"");
        //            sb.Append(url.Replace(pagePlaceHolder, (pagedList.PageIndex + 2).ToString()));
        //            sb.Append("\" title=\"Go to Next Page\">next</a></li>");
        //        }

        //        sb.Append("</ul>");
        //    }

        //    return sb.ToString();
        //}
    }
}
