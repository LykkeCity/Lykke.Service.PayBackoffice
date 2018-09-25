using System.Linq;

namespace BackOffice.Areas.LykkePay.Models
{
    public class ContinuationFormViewModel
    {
        public const string NextContinuationToken = "NextContinuationToken";

        public int PageSize { get; set; }

        public int[] PageSizes { get; set; }

        public string ContinuationToken { get; set; }

        public string DataUrl { get; set; }

        public ContinuationFormViewModel()
        {
            PageSizes = new[] { 10, 20, 30, 40, 50 };
            PageSize = PageSizes.Last();
        }
    }
}
