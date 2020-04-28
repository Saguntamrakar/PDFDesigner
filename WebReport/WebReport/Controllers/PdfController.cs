using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WebReport.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PdfController : ControllerBase
    {
        private readonly PdfConverter pdfConverter;

        public PdfController(PdfConverter pdfConverter)
        {
            this.pdfConverter = pdfConverter;
        }

        [HttpPost]
        public IActionResult getpdf(PdfReportParameter rptParameter)
        {
            //var rparam = JsonConvert.DeserializeObject<PdfReportParameter>(rptParameter.ToString());
            //PdfReportParameter rparam = rptParameter as PdfReportParameter;
            var res = pdfConverter.OpenPdfReport(rptParameter);
            if (res.status  == "ok")
            {
                MemoryStream memStream = new MemoryStream((byte[])res.result);
                return File((byte[])res.result, "application/pdf", "pdf1.pdf");
            }
            return new BadRequestObjectResult(res);
        }
    }
}