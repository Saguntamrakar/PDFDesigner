using iText.IO.Font.Constants;
using iText.IO.Util;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System;
using System.Collections.Generic;
using System.IO;

namespace PDfConsole
{
    class Program
    {
        public class C02E03_StarWars
        {
            public const String DEST = "E:/invoice1.pdf";

            public static void Main(String[] args)
            {
                FileInfo file = new FileInfo(DEST);
                Invoice inv = new Invoice();
                inv.NewHeader();
                inv.NewColumn(inv.Headers[0]);
                inv.Headers[0].Columns[0].width = 2;
                inv.Headers[0].Columns[0].Text = "What is your name";
                inv.Headers[0].Columns[0].IsBold = true;
                inv.Headers[0].Columns[0].FontSize = 14;
                inv.Headers[0].Columns[0].NoLeftBorder = true;
                inv.NewColumn(inv.Headers[0]);
                inv.Headers[0].Columns[1].width = 5;
                inv.Headers[0].Columns[1].Text = "This is my name";
                inv.Headers[0].Columns[1].IsBold = true;
                inv.Headers[0].Columns[1].NoBorder = true;
                inv.Headers[0].Columns[1].FontSize = 7;
                inv.NewColumn(inv.Headers[0]);
                inv.Headers[0].Columns[2].width = 5;
                inv.Headers[0].Columns[2].Text = "This is my name";
                inv.Headers[0].Columns[2].IsBold = true;
                inv.Headers[0].Columns[2].FontSize = 14;
                inv.NewHeader();
                inv.NewColumn(inv.Headers[1]);
                inv.Headers[1].Columns[0].width = 2;
                inv.Headers[1].Columns[0].Text = "What is your name";
                inv.Headers[1].Columns[0].IsBold = true;
                inv.Headers[1].Columns[0].FontSize = 14;
                inv.NewColumn(inv.Headers[1]);
                inv.Headers[1].Columns[1].width = 5;
                inv.Headers[1].Columns[1].Text = "This is my name";
                inv.Headers[1].Columns[1].IsBold = true;
                inv.Headers[1].Columns[1].NoBorder = true;
                inv.Headers[1].Columns[1].FontSize = 14;
                inv.NewColumn(inv.Headers[1]);
                inv.Headers[1].Columns[2].width = 5;
                inv.Headers[1].Columns[2].Text = "This is my name";
                inv.Headers[1].Columns[2].IsBold = true;
                inv.Headers[1].Columns[2].FontSize = 14;
                InoicePrinting inoicePrinting = new InoicePrinting(DEST, "A4");
                inoicePrinting.PrintInvoice(inv,"E;/testpdf.pdf");
                //file.Directory.Create();
                //new C02E03_StarWars().CreatePdfcrawl2(DEST);
            }

        }
    }
}
