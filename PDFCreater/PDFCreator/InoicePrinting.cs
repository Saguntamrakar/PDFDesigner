using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using System.IO;
using System.Linq;

namespace PDfConsole
{
    public class InoicePrinting
    {
        private PdfDocument pdf;
        private PageSize ps;
        private Document document;
        private PdfFont font;
        private PdfFont bold;
        string dest = "E:/invoce.pdf";
        public InoicePrinting(string PaperSize, string customSize = "")
        {


            
            // Initialize document

            font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            bold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        }
        public void PrintInvoice(Invoice invoice, string filename)
        {
            ps = GetPaperSize(invoice.Document.PaperSize,invoice.Document.CustomSize);
            dest = filename;
            pdf = new PdfDocument(new PdfWriter(dest));
            document = new Document(pdf, ps);
            CreatePDF(invoice);
        }
        //public Stream GetMemoryOfInvoice(Invoice invoice)
        //{
        //    Stream memStream = new str;
        //    PdfWriter pdfWriter = new PdfWriter(memStream);
        //    pdf = new PdfDocument(pdfWriter);
        //    document = new Document(pdf, ps);
        //    CreatePDF(invoice);
        //    return memStream;
        //}
        public void CreatePDF(Invoice invoice)
        {
            try
            {
                foreach (iHeader header in invoice.Headers)
                {
                    float[] cols = header.Columns.Select(x => x.width).ToArray();
                    Table table = new Table(UnitValue.CreatePercentArray(cols));
                    foreach (iHeaderColumn col in header.Columns)
                    {
                        Cell cell = new Cell().Add(new Paragraph(col.Text));
                        cell.SetFont(GetPdfFont(col.FontName));
                        cell.SetFontSize(col.FontSize);
                        if (col.IsBold == true) cell.SetBold();
                        if (col.NoBorder == true)
                        {
                            cell.SetBorder(Border.NO_BORDER);
                        }
                        else
                        {
                            cell.SetBorder(Border.NO_BORDER);
                            if (col.NoBottomBorder == false) cell.SetBorderBottom(new SolidBorder(col.BorderWidth)); else cell.SetBorderBottom(Border.NO_BORDER);
                            if (col.NoRightBorder == false) cell.SetBorderRight(new SolidBorder(col.BorderWidth)); else cell.SetBorderRight(Border.NO_BORDER);
                            if (col.NoTopBorder == false) cell.SetBorderTop(new SolidBorder(col.BorderWidth)); else cell.SetBorderTop(Border.NO_BORDER);
                            if (col.NoLeftBorder == false) cell.SetBorderLeft(new SolidBorder(col.BorderWidth)); else cell.SetBorderLeft(Border.NO_BORDER);
                        }
                        cell.SetTextAlignment(GetTextAlignment(col.TextAlignment));
                        cell.SetHorizontalAlignment(GetHorizontalAlignment(col.HorizontalAlignment));
                        cell.SetVerticalAlignment(GetVerticalAlignment(col.verticalAlignment));
                        table.AddCell(cell);

                    }
                    document.Add(table);
                }
                document.Close();
            }
            catch (System.Exception ex)
            {


            }
        }
        public TextAlignment GetTextAlignment(iTextAlignment alignment)
        {
            switch (alignment)
            {
                case iTextAlignment.Center:
                    return TextAlignment.CENTER;
                case iTextAlignment.Right:
                    return TextAlignment.RIGHT;
                case iTextAlignment.Justified:
                    return TextAlignment.JUSTIFIED;
                case iTextAlignment.JustifiedAll:
                    return TextAlignment.JUSTIFIED_ALL;
                default:
                    return TextAlignment.LEFT;

            }
        }
        public HorizontalAlignment GetHorizontalAlignment(iHorizontalAlignment horizontalAlignment)
        {
            switch (horizontalAlignment)
            {
                case iHorizontalAlignment.Left:
                    return HorizontalAlignment.LEFT;
                case iHorizontalAlignment.Center:
                    return HorizontalAlignment.CENTER;
                case iHorizontalAlignment.Right:
                    return HorizontalAlignment.LEFT;
                default:
                    return HorizontalAlignment.LEFT;
            }
        }
        public VerticalAlignment GetVerticalAlignment(iVerticalAlignment verticalAlignment)
        {
            switch (verticalAlignment)
            {
                case iVerticalAlignment.Bottom:
                    return VerticalAlignment.BOTTOM;
                case iVerticalAlignment.Middle:
                    return VerticalAlignment.MIDDLE;
                case iVerticalAlignment.Top:
                    return VerticalAlignment.TOP;
                default:
                    return VerticalAlignment.BOTTOM;
            }
        }
        public PdfFont GetPdfFont(string fontname)
        {
            if (fontname == null) fontname = "";
            switch (fontname.ToUpper())
            {
                case "HELVETICA":
                    return PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                case "HELVETICA_BOLD":
                    return PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                case "TIMES_ROMAN":
                    return PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
                case "COURIER":
                    return PdfFontFactory.CreateFont(StandardFonts.COURIER);
                case "COURIER_BOLD":
                    return PdfFontFactory.CreateFont(StandardFonts.COURIER_BOLD);
                case "COURIER_BOLDOBLIQUE":
                    return PdfFontFactory.CreateFont(StandardFonts.COURIER_BOLDOBLIQUE);
                case "TIMES_BOLDITALIC":
                    return PdfFontFactory.CreateFont(StandardFonts.TIMES_BOLDITALIC);
                case "TIMES_ITALIC":
                    return PdfFontFactory.CreateFont(StandardFonts.TIMES_ITALIC);
                case "SYMBOL":
                    return PdfFontFactory.CreateFont(StandardFonts.SYMBOL);
                default:
                    return PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            }
        }

        public PageSize GetPaperSize(iPaperSize paperSize, string CustomeSize = "")
        {
            
            switch (paperSize)
            {
                case iPaperSize.A4:
                    return PageSize.A4;
                case iPaperSize.Custom:
                    if (string.IsNullOrEmpty(CustomeSize)) return PageSize.A4;
                    var str = CustomeSize.ToLower().Split('x');
                    if (str.Count() < 2) return PageSize.A4;
                    float.TryParse(str[0], out float width);
                    float.TryParse(str[1], out float height);
                    return new PageSize(width, height);
                case iPaperSize.A2:
                    return PageSize.A2;
                case iPaperSize.A3:
                    return PageSize.A3;
                case iPaperSize.A5:
                    return PageSize.A5;
                case iPaperSize.A6:
                    return PageSize.A6;
                case iPaperSize.A7:
                    return PageSize.A7;
                case iPaperSize.A8:
                    return PageSize.A8;
                case iPaperSize.A9:
                    return PageSize.A9;
                case iPaperSize.A10:
                    return PageSize.A10;
                default:
                    return PageSize.A4;
            }
        }
    }
}

