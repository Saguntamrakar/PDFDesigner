using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebReport
{
    public class PdfReportParameter
    {
        public string filename { get; set; }
        public IDictionary<string,Object> Parameter { get; set; }
    }
}
