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

    public class PdfReportMultiParameter
    {
        public string filename { get; set; }
        public List<IDictionary<string, Object>> Parameters { get; set; }
        //if sqldata is same but the titles and other infor are different then set IssameDataQuery to true
        public bool IsSameDataQuery { get; set; }
    }
}
