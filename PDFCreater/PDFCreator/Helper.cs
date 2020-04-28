﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Layout.Renderer;
using iText.Layout.Layout;
using iText.Kernel.Pdf.Xobject;
namespace PDfCreator.Helper
{
    public class TruncateTextInCell
    {
        public static readonly string DEST = "results/sandbox/tables/truncate_text_in_cell.pdf";

        //public static void Main(String[] args)
        //{
        //   FileInfo file = new FileInfo(DEST);
        //    file.Directory.Create();

        //    new TruncateTextInCell().ManipulatePdf(DEST);
        //}

        private void ManipulatePdf(String dest)
        {
           PdfDocument pdfDoc = new PdfDocument(new PdfWriter(dest));
            Document doc = new Document(pdfDoc);

            Table table = new Table(UnitValue.CreatePercentArray(5)).UseAllAvailableWidth();

            for (int r = 'A'; r <= 'Z'; r++)
            {
                for (int c = 1; c <= 5; c++)
                {
                    Cell cell = new Cell();
                    if (r == 'D' && c == 2)
                    {
                        cell.SetNextRenderer(new FitCellRenderer(cell,
                            "D2 is a cell with more content than we can fit into the cell."));
                    }
                    else
                    {
                        cell.Add(new Paragraph(((char)r).ToString() + c));
                    }

                    table.AddCell(cell);
                }
            }

            doc.Add(table);

            doc.Close();
        }

        
    }

    public  class FitCellRenderer : CellRenderer
    {
        private String content;

        public FitCellRenderer(Cell modelElement, String content)
            : base(modelElement)
        {
            this.content = content;
        }

        // If renderer overflows on the next area, iText uses getNextRender() method to create a renderer for the overflow part.
        // If getNextRenderer isn't overriden, the default method will be used and thus a default rather than custom
        // renderer will be created
        public override IRenderer GetNextRenderer()
        {
            return new FitCellRenderer((Cell)modelElement, content);
        }

        /**
         * Method adapts content, that can't be fit into the cell,
         * to prevent truncation by replacing truncated part of content with '...'
         */
        public override LayoutResult Layout(LayoutContext layoutContext)
        {
            PdfFont bf = GetPropertyAsFont(Property.FONT);
            int contentLength = content.Length;
            int leftCharIndex = 0;
            int rightCharIndex = contentLength - 1;

            Rectangle rect = layoutContext.GetArea().GetBBox().Clone();

            // Cell's margins, borders and paddings should be extracted from the available width as well.
            // Note that this part of the sample was introduced specifically for iText7.
            // since in iText5 the approach of processing cells was different
            ApplyBordersPaddingsMargins(rect, GetBorders(), GetPaddings());
            float availableWidth = rect.GetWidth();

            UnitValue fontSizeUV = this.GetPropertyAsUnitValue(Property.FONT_SIZE);

            // Unit values can be of POINT or PERCENT type. In this particular sample
            // the font size value is expected to be of POINT type.
            float fontSize = fontSizeUV.GetValue();

            availableWidth -= bf.GetWidth("...", fontSize);

            while (leftCharIndex < contentLength && rightCharIndex != leftCharIndex)
            {
                availableWidth -= bf.GetWidth(content[leftCharIndex], fontSize);
                if (availableWidth > 0)
                {
                    leftCharIndex++;
                }
                else
                {
                    break;
                }

                availableWidth -= bf.GetWidth(content[rightCharIndex], fontSize);

                if (availableWidth > 0)
                {
                    rightCharIndex--;
                }
                else
                {
                    break;
                }
            }

            // left char is the first char which should not be added
            // right char is the last char which should not be added
            String newContent = content.Substring(0, leftCharIndex) + "..." + content.Substring(rightCharIndex + 1);
            Paragraph p = new Paragraph(newContent);

            // We're operating on a Renderer level here, that's why we need to process a renderer,
            // created with the updated paragraph
            IRenderer pr = p.CreateRendererSubTree().SetParent(this);
            childRenderers.Add(pr);

            return base.Layout(layoutContext);
        }


    }

    public class TruncateCellRenderer : CellRenderer
    {
        private String content;

        public TruncateCellRenderer(Cell modelElement, String content)
            : base(modelElement)
        {
            this.content = content;
        }

        // If renderer overflows on the next area, iText uses getNextRender() method to create a renderer for the overflow part.
        // If getNextRenderer isn't overriden, the default method will be used and thus a default rather than custom
        // renderer will be created
        public override IRenderer GetNextRenderer()
        {
            return new FitCellRenderer((Cell)modelElement, content);
        }

        /**
         * Method adapts content, that can't be fit into the cell,
         * to prevent truncation by replacing truncated part of content with '...'
         */
        public override LayoutResult Layout(LayoutContext layoutContext)
        {
            PdfFont bf = GetPropertyAsFont(Property.FONT);
            int contentLength = content.Length;
            int leftCharIndex = 0;
            int rightCharIndex = contentLength - 1;

            Rectangle rect = layoutContext.GetArea().GetBBox().Clone();

            // Cell's margins, borders and paddings should be extracted from the available width as well.
            // Note that this part of the sample was introduced specifically for iText7.
            // since in iText5 the approach of processing cells was different
            ApplyBordersPaddingsMargins(rect, GetBorders(), GetPaddings());
            float availableWidth = rect.GetWidth();

            UnitValue fontSizeUV = this.GetPropertyAsUnitValue(Property.FONT_SIZE);

            // Unit values can be of POINT or PERCENT type. In this particular sample
            // the font size value is expected to be of POINT type.
            float fontSize = fontSizeUV.GetValue();

            availableWidth -= bf.GetWidth("...", fontSize);

            while (leftCharIndex < contentLength )
            {
                availableWidth -= bf.GetWidth(content[leftCharIndex], fontSize);
                if (availableWidth > 0)
                {
                    leftCharIndex++;
                }
                else
                {
                    break;
                }

                //availableWidth -= bf.GetWidth(content[rightCharIndex], fontSize);

                //if (availableWidth > 0)
                //{
                //    rightCharIndex--;
                //}
                //else
                //{
                //    break;
                //}
            }

            // left char is the first char which should not be added
            // right char is the last char which should not be added
            string newContent;
            if(leftCharIndex >=content.Length - 1)
            {
                newContent = content;
            }
            else
            {
                newContent = content.Substring(0, leftCharIndex) + "...";
            }
            Paragraph p = new Paragraph(newContent);

            // We're operating on a Renderer level here, that's why we need to process a renderer,
            // created with the updated paragraph
            IRenderer pr = p.CreateRendererSubTree().SetParent(this);
            childRenderers.Add(pr);

            return base.Layout(layoutContext);
        }


    }

    public class ClipCenterCellContent
    {
        public static readonly string DEST = "results/sandbox/tables/clip_center_cell_content.pdf";

        public static void Main(String[] args)
        {
            FileInfo file = new FileInfo(DEST);
            file.Directory.Create();

            new ClipCenterCellContent().ManipulatePdf(DEST);
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
                    if (r == 'D' && c == 2)
                    {
                        // Draw a content that will be clipped in the cell 
                        cell.SetNextRenderer(new ClipCenterCellContentCellRenderer(cell,
                            new Paragraph("D2 is a cell with more content than we can fit into the cell.")));
                    }
                    else
                    {
                        cell.Add(new Paragraph(((char)r).ToString() + c));
                    }

                    table.AddCell(cell);
                }
            }

            doc.Add(table);

            doc.Close();
        }

        
    }
    public class ClipCenterCellContentCellRenderer : CellRenderer
    {
        private Paragraph content;

        public ClipCenterCellContentCellRenderer(Cell modelElement, Paragraph content)
            : base(modelElement)
        {
            this.content = content;
        }

        // If renderer overflows on the next area, iText uses getNextRender() method to create a renderer for the overflow part.
        // If getNextRenderer isn't overriden, the default method will be used and thus a default rather than custom
        // renderer will be created
        public override IRenderer GetNextRenderer()
        {
            return new ClipCenterCellContentCellRenderer((Cell)modelElement, content);
        }

        public override void Draw(DrawContext drawContext)
        {

            // Fictitiously layout the renderer and find out, how much space does it require
            IRenderer pr = content.CreateRendererSubTree().SetParent(this);

            LayoutResult textArea = pr.Layout(new LayoutContext(
                new LayoutArea(0, new Rectangle(GetOccupiedAreaBBox().GetWidth(), 1000))));

            float spaceNeeded = textArea.GetOccupiedArea().GetBBox().GetHeight();
            Console.WriteLine("The content requires {0} pt whereas the height is {1} pt.",
                spaceNeeded, GetOccupiedAreaBBox().GetHeight());

            float offset = (GetOccupiedAreaBBox().GetHeight() - textArea.GetOccupiedArea()
                                .GetBBox().GetHeight()) / 2;
            Console.WriteLine("The difference is {0} pt; we'll need an offset of {1} pt.",
                -2f * offset, offset);

            PdfFormXObject xObject = new PdfFormXObject(new Rectangle(GetOccupiedAreaBBox().GetWidth(),
                GetOccupiedAreaBBox().GetHeight()));

            Canvas layoutCanvas = new Canvas(new PdfCanvas(xObject, drawContext.GetDocument()),
                drawContext.GetDocument(),
                new Rectangle(0, offset, GetOccupiedAreaBBox().GetWidth(), spaceNeeded));
            layoutCanvas.Add(content);

            drawContext.GetCanvas().AddXObject(xObject, occupiedArea.GetBBox().GetLeft(),
                occupiedArea.GetBBox().GetBottom());
        }
    }
}
