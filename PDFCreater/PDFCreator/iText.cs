using iText.IO.Font.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace PDfConsole
{
    public class iText
    {
        public double X { get; set; }
        public double Y { get; set; }
        public StandardFonts Font { get; set; }

        public int FontSize { get; set; }

        public string text { get; set; }

    }
    public class iLine
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public class iTextLine
    {
        public List<iText> texts { get; set; }
        public List<iLine> lines { get; set; }
    }
    public class iParagraph
    {
        public string Text { get; set; }

    }

    public class iTable
    {
        public List<float> Columns { get; set; }
        public string[] Data { get; set; }
    }
    public class iDocument
    {
        public iPaperSize PaperSize { get; set; } = iPaperSize.A4;
        public string CustomSize { get; set; } = "";
    }
    public class Column
    {
        public float width { get; set; }
        public iColor BackgroundColor { get; set; }
        public float BoarderWidth { get; set; }

    }

    public class iColor
    {
        public iColor()
        {

        }
        public iColor(float c, float y, float m, float k)
        {
            C = c; Y = y; M = m; K = k;
        }
        public float C { get; set; }
        public float Y { get; set; }
        public float M { get; set; }
        public float K { get; set; }
    }

    public class iHeaders
    {
        public List<iHeader> HeaderList { get; set; }
        public iHeaders()
        {
            HeaderList = new List<iHeader>();
        }
    }
    public class iHeader
    {

        public List<iHeaderColumn> Columns { get; set; }
        public iHeader()
        {

            Columns = new List<iHeaderColumn>();
        }


    }
    public class iHeaderColumn
    {

        public float width { get; set; } = 1;
        public iColor BackgroundColor { get; set; }
        public float BoarderWidth { get; set; }
        public string Text { get; set; }
        public int FontSize { get; set; } = 14;
        public bool IsBold { get; set; }
        public float BorderWidth { get; set; } = 1;
        public string FontName { get; set; }
        public bool NoBorder { get; set; }
        public bool NoLeftBorder { get; set; }
        public bool NoRightBorder { get; set; }
        public bool NoTopBorder { get; set; }
        public bool NoBottomBorder { get; set; }
        public iTextAlignment TextAlignment { get; set; } = iTextAlignment.Left;
        public iHorizontalAlignment HorizontalAlignment { get; set; } = iHorizontalAlignment.Left;
        public iVerticalAlignment verticalAlignment { get; set; } = iVerticalAlignment.Middle;

    }
    public enum  iTextAlignment
    {
        Center =1,
        Left=0,
        Right =2,
        Justified=3,
        JustifiedAll=4
    }

    public enum iHorizontalAlignment
    {
        Center,Left,Right
    }
    public enum iVerticalAlignment
    {
        Top,Middle,Bottom
    }

    public enum iPaperSize
    {
        A4,A5,A2,A3,A6,A7,A8,A9,A10,Custom
    }
}
