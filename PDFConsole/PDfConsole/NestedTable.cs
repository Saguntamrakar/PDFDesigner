using System;
using System.IO;
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
namespace PDfConsole
{
    public class NestedTableProblem
    {
        public static readonly string DEST = "E:/nestedTable.pdf";

        //public static void Main(String[] args)
        //{
        //    FileInfo file = new FileInfo(DEST);
        //    file.Directory.Create();

        //    new NestedTableProblem().ManipulatePdf(DEST);
        //}

        private void ManipulatePdf(string dest)
        {
            PdfDocument pdfDoc = new PdfDocument(new PdfWriter(dest));
            Document doc = new Document(pdfDoc, new PageSize(612, 792));

            doc.SetMargins(30, 21, 35, 21);

            // inner table
            Table innerTable = new Table(UnitValue.CreatePercentArray(1));
            innerTable.SetWidth(UnitValue.CreatePercentValue(80));
            innerTable.SetHorizontalAlignment(HorizontalAlignment.RIGHT);
            innerTable.AddCell(new Cell().SetBorder(Border.NO_BORDER)
                .Add(new Paragraph("Goodbye World"))) ;

            // outer table
            Table outerTable = new Table(UnitValue.CreatePercentArray(1)).UseAllAvailableWidth();
            outerTable.SetHorizontalAlignment(HorizontalAlignment.LEFT).SetBorder(Border.NO_BORDER);

            Cell cell = new Cell();
            cell.SetBorder(Border.NO_BORDER);
            cell.Add(new Paragraph("Hello World"));
            cell.Add(innerTable);
            cell.Add(new Paragraph("Hello World"));

            outerTable.AddCell(cell);
            doc.Add(outerTable);

            doc.Close();
        }
    }
}
