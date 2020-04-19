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
            C = c;Y = y;M = m;K = k;
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
        private Column col;
        public iHeaderColumn()
        {
            col = new Column();
        }
        public Column Column { get { return col; } }
        public string Text { get; set; }
        public int FontSize { get; set; } = 14;
        public bool IsBold { get; set; }
        public float BorderWidth { get; set; } = 1;
        public string FontName { get; set; } 
        public bool NoBorder { get; set; }
        public bool NoLeftBorder { get; set; }
        public  bool NoRightBorder { get; set; }
        public bool NoTopBorder { get; set; }
        public bool NoBottomBorder { get; set; }
        
    }
}
