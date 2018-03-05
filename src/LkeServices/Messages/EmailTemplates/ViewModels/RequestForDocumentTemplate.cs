using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LkeServices.Messages.EmailTemplates.ViewModels
{
    public class RequestForDocumentTemplate
    {

        public string FullName { get; set; }
        public string Text { get; set; }
        public string Comment { get; set; }
        public int Year { get; set; }
    }
}
