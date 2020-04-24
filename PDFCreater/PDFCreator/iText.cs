using iText.IO.Font.Constants;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json.Linq;
using  System.Dynamic;

namespace PDfCreator.Models
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


    public class iDocument
    {
        private string _DetailSource = "";
        private string _ReportSource = "";
        private string _ConnectionString="";
        private string _QueryParameter;
        private JObject _ReportDataObject;
        private List<JObject> _DetailDataObject;
        private ExpandoObject _Parameter;
        public iPaperSize PaperSize { get; set; } = iPaperSize.A4;
        public string Margin { get; set; }
        public string CustomSize { get; set; } = "";
        public iOreintation Oreintation { get; set; } = iOreintation.Portrait;
        [DataMemberAttribute]
        public string DetailSource { get { return _DetailSource; } }
        public string ReportSource { get { return _ReportSource; } }
        public string ConnectionString { get { return _ConnectionString; } }
        public string QueryParameter { get { return _QueryParameter; } }
        public string DetailFields { get; set; }
        
        public DataType DetailDataType { get; set; } = DataType.csv;
        public void setDetailSource(string source)
        {
            _DetailSource = source;
        }
        
        
        public void setReportSource(string source)
        {
            _ReportSource = source;
        }
        public void setSqlConnection(string source)
        {
            _ConnectionString = source;
        }
        public void setReportData(JObject obj)
        {
            _ReportDataObject = obj;
        }
        public void setDetailData(List<JObject> obj)
        {
            _DetailDataObject = obj;
        }
        public void setQueryParameter(string obj)
        {
            _QueryParameter = obj;
        }
        public void setParameter(ExpandoObject obj)
        {
            _Parameter = obj;
        }
        public ExpandoObject getParameter()
        {
            return _Parameter;
        }
        public iDocument DeepCopy()
        {
            iDocument doc = (iDocument)this.MemberwiseClone();

            return doc;
        }

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
        public List<iTable> HeaderList { get; set; }
        public iColumnArrayUnit ColumnArrayUnit { get; set; } = iColumnArrayUnit.PercentArray;
        public iHeaders()
        {
            HeaderList = new List<iTable>();
        }
    }
    public class iDetail
    {
        iTable _DetailHeader;
        iTable _DetailFooter;
        iFixedColTable _Detail;
        private float[] columns { get; set; }
        private string _ColWidths = "";
        public string ColWidths { get { return _ColWidths; } set { _ColWidths = value; SetColumns(); } }
        public iColumnArrayUnit ColumnArrayUnit { get; set; } = iColumnArrayUnit.PercentArray;
        public iTable DetailHeader { get { return _DetailHeader; } }
        public iFixedColTable Detail { get { return _Detail; } }
        public iTable DetailFooter { get { return _DetailFooter; } }
        public iDetail()
        {
            _DetailHeader = new iTable();
            _DetailFooter = new iTable();
        }
        private void SetColumns()
        {
            var cols = _ColWidths.Split(',');
            List<float> colwidths = new List<float>();
            if (cols.Length > 0)
            {
                for (int i = 0; i < cols.Length; i++)
                {
                    float.TryParse(cols[i], out float fCol);
                    colwidths.Add(fCol);
                }
                columns = colwidths.ToArray();
                _Detail = new iFixedColTable(colwidths.ToArray());

            }
        }
        public float[] getColWidths()
        {
            return columns;
        }
        public iDetail DeepCopy()
        {
            iDetail detail = (iDetail)this.MemberwiseClone();
            return detail;
        }

    }
    public class iTable
    {
        private string _ColWidths = "";
        private float[] _widths;
        public string ColWidths { get { return _ColWidths; } set { _ColWidths = value; SetColumns(); } }
        public float TableWidth { get; set; }
        public iHorizontalAlignment HorizontalAlignment { get; set; } = iHorizontalAlignment.Left;
        public iVerticalAlignment VerticalAlignment { get; set; } = iVerticalAlignment.Middle;
        public bool IsFixedLayout { get; set; }
        public bool UserAllAvailableWidth { get; set; }
        public ArrayList Columns { get; set; }
        public iColumnArrayUnit ColumnArrayUnit { get; set; } = iColumnArrayUnit.PercentArray;
        public iTable()
        {

            Columns = new ArrayList();
        }
        private void SetColumns()
        {
            var cols = _ColWidths.Split(',');
            List<float> colwidths = new List<float>();
            if (cols.Length > 0)
            {
                for (int i = 0; i < cols.Length; i++)
                {
                    float.TryParse(cols[i], out float fCol);
                    colwidths.Add(fCol);
                }
                _widths = colwidths.ToArray();


            }
        }
        public float[] getColWidths()
        {
            return _widths;
        }
        public void setColWidths(float[] cols)
        {
            //var strWidths= string.Join(",", cols.Select(x => x.ToString()));
            //_ColWidths = strWidths;
        }
        public iTable DeepCopy()
        {
            iTable tbl = (iTable)this.MemberwiseClone();
            return tbl;
        }

    }
    public class iFixedColTable
    {
        List<iColumn> _Columns;
        public IReadOnlyList<iColumn> Columns { get { return _Columns.AsReadOnly(); } }
        public iColumnArrayUnit ColumnArrayUnit { get; set; } = iColumnArrayUnit.PercentArray;
        public int FixedRows { get; set; }
        public iFixedColTable(float[] cols)
        {
            _Columns = new List<iColumn>();
            for (int i = 0; i < cols.Length; i++)
            {
                _Columns.Add(new iColumn { width = cols[i] });
            }

        }


    }
    public class iColumn
    {

        public float width { get; set; } = 1;
        public iColor BackgroundColor { get; set; }
        public float BoarderWidth { get; set; }
        public string Text { get; set; }
        public int FontSize { get; set; } = 14;
        public bool IsBold { get; set; }
        public float BorderWidth { get; set; } = 1;
        public iFonts FontName { get; set; } = iFonts.HELVETICA;
        public bool NoBorder { get; set; }
        public bool NoLeftBorder { get; set; }
        public bool NoRightBorder { get; set; }
        public bool NoTopBorder { get; set; }
        public bool NoBottomBorder { get; set; }
        public bool IsItalic { get; set; }
        public int ColSpan { get; set; }
        public int RowSpan { get; set; }
        public float height { get; set; }
        public iTextAlignment TextAlignment { get; set; } = iTextAlignment.Left;
        public iHorizontalAlignment HorizontalAlignment { get; set; } = iHorizontalAlignment.Left;
        public iVerticalAlignment VerticalAlignment { get; set; } = iVerticalAlignment.Middle;
        public int MaxChar { get; set; } = 0;

    }
    public enum iTextAlignment
    {
        Center = 1,
        Left = 0,
        Right = 2,
        Justified = 3,
        JustifiedAll = 4
    }

    public enum iHorizontalAlignment
    {
        Center, Left, Right
    }
    public enum iVerticalAlignment
    {
        Top, Middle, Bottom
    }

    public enum iPaperSize
    {
        A4, A5, A2, A3, A6, A7, A8, A9, A10, Custom
    }
    public enum iOreintation
    {
        Portrait, Landscape
    }
    public enum iFonts
    {
        HELVETICA, HELVETICA_BOLD, TIMES_ROMAN, COURIER, COURIER_BOLD, COURIER_BOLDOBLIQUE,
        TIMES_BOLDITALIC, TIMES_ITALIC, SYMBOL
    }
    public enum iColumnArrayUnit
    {
        PointArray, PercentArray
    }

    public enum DataType
    {
        csv, SQLServer
    }
    public enum TableType
    {
        table, header, footer
    }

    
}
