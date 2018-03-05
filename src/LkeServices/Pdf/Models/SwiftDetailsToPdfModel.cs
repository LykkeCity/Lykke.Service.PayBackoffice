using System;
using System.Collections.Generic;
using System.Text;

namespace LkeServices.Pdf.Models
{
    public class SwiftDetailsToPdfModel
    {
        public string RequestId { get; set; }
        public DateTime DateOfTransaction { get; set; }
        public string ClientId { get; set; }
    }
}
