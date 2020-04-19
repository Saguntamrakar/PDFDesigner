using System;
using iText.Kernel;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;

namespace PDFCreator
{
    public class PdfCreator

    {
        public void PdfCreate(string filename)
        {

            PdfWriter pdfWriter = new PdfWriter(filename);
            PdfDocument document = new PdfDocument(pdfWriter);
            PageSize pageSize = PageSize.A4;
            PdfPage pdfpage = document.AddNewPage(pageSize);
            PdfCanvas canvas = new PdfCanvas(pdfpage);
            

        }
    }
}
