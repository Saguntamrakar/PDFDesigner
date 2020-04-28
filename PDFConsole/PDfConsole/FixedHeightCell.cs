﻿using System;
using System.IO;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace PDfConsole
{
    public class FixedHeightCell
    {
        public static readonly string DEST = "E:/FixedHeightcell.pdf";

        public static void Main(String[] args)
        {
            FileInfo file = new FileInfo(DEST);
            file.Directory.Create();

            new FixedHeightCell().ManipulatePdf(DEST);
        }

        private void ManipulatePdf(string dest)
        {
            PdfDocument pdfDoc = new PdfDocument(new PdfWriter(dest));
            Document doc = new Document(pdfDoc);

            Table table = new Table(UnitValue.CreatePercentArray(5)).UseAllAvailableWidth();

            for (int r = 'A'; r <= 'Z'; r++)
            {
                for (int c = 1; c <= 5; c++)
                {
                    Cell cell = new Cell();
                    cell.Add(new Paragraph(((char)r).ToString() + c));

                    if (r == 'D')
                    {
                        cell.SetHeight(60);
                    }
                    if (r == 'E')
                    {
                        cell.SetHeight(60);
                        if (c == 4)
                        {
                            cell.SetHeight(120);
                        }
                    }
                    if (r == 'F')
                    {
                        cell.SetMinHeight(60);
                        cell.SetHeight(60);
                        if (c == 2)
                        {
                            cell.Add(new Paragraph("This cell has more content than the other cells"));
                        }
                    }

                    table.AddCell(cell);
                }
            }

            doc.Add(table);

            doc.Close();
        }
    }
}