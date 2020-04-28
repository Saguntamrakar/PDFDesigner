using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using PDfCreator.Models;
using PDfCreator.Helper;
using System.Data.SqlClient;
using Dapper;
using System.Dynamic;
namespace PDfCreator.Print
{
    public class InvoicePrinting
    {
        private PdfDocument pdf;
        private PageSize ps;
        private Document document;
        private PdfFont font;
        private PdfFont bold;
        string dest = "E:/invoce.pdf";
        private string _InputParameter;
        public List<IDictionary<string, object>> DetailData { get; set; }
        public IDictionary<string, object> ReportData { get; set; }
        public IDictionary<string, object> InputParameters { get; set; }
        private int FixedRows;
        private bool IsLastPage;

        public InvoicePrinting()
        {



            // Initialize document

            font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            bold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        }
        public void PrintInvoice(Invoice invoice, string filename, string inputJsonParameter = "")
        {
            try
            {

                ps = GetPaperSize(invoice.Document.PaperSize, invoice.Document.CustomSize);
                dest = filename;
                pdf = new PdfDocument(new PdfWriter(dest));
                if (invoice.Document.Oreintation == iOreintation.Landscape)
                {
                    document = new Document(pdf, ps.Rotate());
                }
                else
                {
                    document = new Document(pdf, ps);
                }
                if (string.IsNullOrEmpty(invoice.Document.Margin) == true) invoice.Document.Margin = "0,0,0,0";
                string[] strMargins = invoice.Document.Margin.Split(',');
                float leftMargin = 0; float topMargin = 0; float rightMargin = 0; float bottomMargin = 0;
                float.TryParse(strMargins[0], out leftMargin);
                if (strMargins.Length > 1) float.TryParse(strMargins[1], out topMargin);
                if (strMargins.Length > 2) float.TryParse(strMargins[2], out rightMargin);
                if (strMargins.Length > 3) float.TryParse(strMargins[3], out bottomMargin);
                document.SetTopMargin(topMargin);
                document.SetBottomMargin(bottomMargin);
                document.SetLeftMargin(leftMargin);
                document.SetRightMargin(rightMargin);
                if (invoice.Detail.Detail != null)
                {
                    FixedRows = invoice.Detail.FixedRows;
                }

                IsLastPage = true;
                if (FixedRows > 0 && DetailData != null && DetailData.Count() > 0)
                {
                    int startRow = 0;



                    while (startRow < DetailData.Count())
                    {
                        IsLastPage = false;
                        if (startRow >= DetailData.Count() - 1)
                        {
                            IsLastPage = true;
                        }
                        CreatePDF(invoice, startRow);
                        startRow = startRow + FixedRows;
                        if (startRow < DetailData.Count())
                        {
                            document.Add(new AreaBreak());

                        }


                    }
                }
                else
                {
                    CreatePDF(invoice, 0);
                }


            }
            catch (Exception ex)
            {
                //document.Close();
                throw ex;
            }
            finally
            {
                document.Close();
            }
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
        public void CreatePDF(Invoice invoice, int startrow)
        {
            try
            {
                foreach (var repHeader in invoice.ReportHeaders)
                {
                    //JObject obj = repHeader as JObject;
                    var header = repHeader as iTable;
                    if (repHeader != null)
                    {
                        float[] tblColwidth = header.getColWidths();
                        float[] cols;
                        if (tblColwidth != null && tblColwidth.Length > 0)
                        {
                            cols = tblColwidth;
                        }
                        else
                        {
                            cols = header.Columns.ToArray().Select(x => { float i = 1; return i; }).ToArray();
                            header.setColWidths(cols);
                        }

                        Table table = CreateTable(header, cols);
                        if (header.TableWidth > 0)
                        {
                            table.SetWidth(MillimetersToPoints(header.TableWidth));
                        }
                        foreach (var col in header.Columns)
                        {
                            if (col.GetType().Equals(typeof(iTable)))
                            {
                                AddTable((iTable)col, table);
                            }
                            else
                            {
                                AddColumn((iColumn)col, table);
                            }


                        }
                        document.Add(table);
                    }
                }
                //Adding Detail section
                if (invoice.Detail.Detail.Columns.Count > 0)
                {
                    float[] tblColwidth = invoice.Detail.getColWidths();
                    float[] cols;
                    if (tblColwidth != null && tblColwidth.Length > 0)
                    {
                        cols = tblColwidth;
                        Table table = CreateTable(invoice.Detail.DetailHeader, cols);
                        table.SetFixedLayout();
                        foreach (var col in invoice.Detail.DetailHeader.Columns)
                        {
                            AddColumn((iColumn)col, table, tableType: TableType.header);
                        }
                        if (invoice.Document.DetailDataType == DataType.csv)
                        {
                            if (File.Exists(invoice.Document.DetailSource))
                            {
                                LoadCsvTable(invoice.Document.DetailSource, invoice.Detail.Detail.Columns, table);
                            }
                            else
                            {
                                LoadBlankTable(invoice.Detail.Detail.Columns, table);
                            }

                        }
                        else if (invoice.Document.DetailDataType == DataType.SQLServer)
                        {
                            //PrepareSqlReportData(invoice, param, invoice.Detail.Detail.Columns.ToList(), table);
                            //LoadSqlTable(DetailData , invoice.Document.DetailFields, invoice.Detail.Detail.Columns.ToList(), table);
                            LoadSqlTableFixedRows(DetailData, invoice.Document.DetailFields, invoice.Detail.Detail.Columns, table, startrow);

                        }
                        foreach (var col in invoice.Detail.DetailFooter.Columns)
                        {
                            AddColumn((iColumn)col, table, tableType: TableType.footer);
                        }
                        table.IsKeepTogether();
                        document.Add(table);

                    }

                    //float[] cols = invoice.Detail.DetailHeader.Columns.ToArray().Select(x => { float i = 1; return i; }).ToArray();

                }
                //Addding Footer section
                foreach (var repFooter in invoice.ReportFooters)
                {
                    if (repFooter.GetType().Equals(typeof(iTable)))
                    {
                        var header = (iTable)repFooter;
                        float[] tblColwidth = header.getColWidths();
                        float[] cols;
                        if (tblColwidth != null && tblColwidth.Length > 0)
                        {
                            cols = tblColwidth;
                        }
                        else
                        {
                            cols = header.Columns.ToArray().Select(x => { float i = 1; return i; }).ToArray();
                            header.setColWidths(cols);
                        }
                        Table table = CreateTable(header, cols);
                        foreach (var col in header.Columns)
                        {
                            if (col.GetType().Equals(typeof(iTable)))
                            {
                                AddTable((iTable)col, table);
                            }
                            else
                            {
                                AddColumn((iColumn)col, table);
                            }

                        }
                        document.Add(table);
                    }
                }

            }
            catch (System.Exception ex)
            {
                throw ex;

            }
            finally
            {

            }
        }

        private Table CreateTable(iTable tbl, float[] cols)
        {
            if (cols.Count() == 0) cols = new float[] { 1 };
            Table table = new Table(GetUnitValue(tbl.ColumnArrayUnit, cols));
            if (tbl.TableWidth > 0)
            {
                table.SetWidth(MillimetersToPoints(tbl.TableWidth));
            }
            table.SetHorizontalAlignment(GetHorizontalAlignment(tbl.HorizontalAlignment));
            table.SetVerticalAlignment(GetVerticalAlignment(tbl.VerticalAlignment));
            if (tbl.UserAllAvailableWidth == true) table.UseAllAvailableWidth();
            if (tbl.IsFixedLayout == true) table.SetFixedLayout();
            if (tbl.NoBorder == true)
            {
                table.SetBorder(Border.NO_BORDER);
            }
            else
            {
                table.SetBorder(Border.NO_BORDER);
                if (tbl.NoBottomBorder == false) table.SetBorderBottom(new SolidBorder(tbl.BorderWidth)); else table.SetBorderBottom(Border.NO_BORDER);
                if (tbl.NoRightBorder == false) table.SetBorderRight(new SolidBorder(tbl.BorderWidth)); else table.SetBorderRight(Border.NO_BORDER);
                if (tbl.NoTopBorder == false) table.SetBorderTop(new SolidBorder(tbl.BorderWidth)); else table.SetBorderTop(Border.NO_BORDER);
                if (tbl.NoLeftBorder == false) table.SetBorderLeft(new SolidBorder(tbl.BorderWidth)); else table.SetBorderLeft(Border.NO_BORDER);
            }
            SettableMargins(table, tbl.Margin);
            SettablePaddings(table, tbl.Padding);
            if (tbl.Height > 0)
            {
                table.SetHeight(tbl.Height);
            }
            return table;
        }

        private void SettableMargins(Table tbl, string margins)
        {
            if (string.IsNullOrEmpty(margins)) { tbl.SetMargin(1); return; }
            string[] strMargins = margins.Split(',');
            float leftMargin = 0; float topMargin = 0; float rightMargin = 0; float bottomMargin = 0;
            float.TryParse(strMargins[0], out leftMargin);
            if (strMargins.Length > 1) float.TryParse(strMargins[1], out topMargin);
            if (strMargins.Length > 2) float.TryParse(strMargins[2], out rightMargin);
            if (strMargins.Length > 3) float.TryParse(strMargins[3], out bottomMargin);
            if (strMargins.Count() == 1)
            {
                tbl.SetMargin(leftMargin);
            }
            else
            {
                tbl.SetMarginTop(topMargin);
                tbl.SetMarginBottom(bottomMargin);
                tbl.SetMarginLeft(leftMargin);
                tbl.SetMarginRight(rightMargin);
            }


        }
        private void SetCellMargins(Cell tbl, string margins)
        {
            if (string.IsNullOrEmpty(margins)) { tbl.SetMargin(1); return; }
            string[] strMargins = margins.Split(',');
            float leftMargin = 0; float topMargin = 0; float rightMargin = 0; float bottomMargin = 0;
            float.TryParse(strMargins[0], out leftMargin);
            if (strMargins.Length > 1) float.TryParse(strMargins[1], out topMargin);
            if (strMargins.Length > 2) float.TryParse(strMargins[2], out rightMargin);
            if (strMargins.Length > 3) float.TryParse(strMargins[3], out bottomMargin);
            if (strMargins.Count() == 0)
            {
                tbl.SetMargin(leftMargin);
            }
            else
            {
                tbl.SetMarginTop(topMargin);
                tbl.SetMarginBottom(bottomMargin);
                tbl.SetMarginLeft(leftMargin);
                tbl.SetMarginRight(rightMargin);
            }


        }
        private void SettablePaddings(Table tbl, string margins)
        {
            if (string.IsNullOrEmpty(margins)) { tbl.SetPadding(1); return; }
            string[] strMargins = margins.Split(',');
            float leftMargin = 0; float topMargin = 0; float rightMargin = 0; float bottomMargin = 0;
            float.TryParse(strMargins[0], out leftMargin);
            if (strMargins.Length > 1) float.TryParse(strMargins[1], out topMargin);
            if (strMargins.Length > 2) float.TryParse(strMargins[2], out rightMargin);
            if (strMargins.Length > 3) float.TryParse(strMargins[3], out bottomMargin);
            if (strMargins.Count() == 0)
            {
                tbl.SetPadding(leftMargin);
            }
            else
            {
                tbl.SetPaddingTop(topMargin);
                tbl.SetPaddingBottom(bottomMargin);
                tbl.SetPaddingLeft(leftMargin);
                tbl.SetPaddingRight(rightMargin);
            }


        }
        private void SetCellPaddings(Cell tbl, string margins)
        {
            if (string.IsNullOrEmpty(margins)) { tbl.SetPadding(1); return; }
            string[] strMargins = margins.Split(',');
            float leftMargin = 0; float topMargin = 0; float rightMargin = 0; float bottomMargin = 0;
            float.TryParse(strMargins[0], out leftMargin);
            if (strMargins.Length > 1) float.TryParse(strMargins[1], out topMargin);
            if (strMargins.Length > 2) float.TryParse(strMargins[2], out rightMargin);
            if (strMargins.Length > 3) float.TryParse(strMargins[3], out bottomMargin);
            if (strMargins.Count() == 0)
            {
                tbl.SetPadding(leftMargin);
            }
            else
            {
                tbl.SetPaddingTop(topMargin);
                tbl.SetPaddingBottom(bottomMargin);
                tbl.SetPaddingLeft(leftMargin);
                tbl.SetPaddingRight(rightMargin);
            }


        }
        //private void AddDetailHeaderColumn(iColumn col, Table table)
        //{
        //    string colText = "";
        //    if (col.MaxChar == 0)
        //        colText = col.Text;
        //    else
        //    {
        //        if (col.Text.Length > col.MaxChar)
        //        {
        //            colText = col.Text.Substring(0, col.MaxChar);
        //        }
        //        else
        //        {
        //            colText = col.Text;
        //        }
        //    }

        //    Cell cell = new Cell(col.RowSpan,col.ColSpan).Add(new Paragraph(colText==null?"":colText));
        //    cell.SetFont(GetPdfFont(col.FontName));
        //    cell.SetFontSize(col.FontSize + 4);
        //    cell.SetBold();
        //    if (col.NoBorder == true)
        //    {
        //        cell.SetBorder(Border.NO_BORDER);
        //    }
        //    else
        //    {
        //        cell.SetBorder(Border.NO_BORDER);
        //        if (col.NoBottomBorder == false) cell.SetBorderBottom(new SolidBorder(col.BorderWidth)); else cell.SetBorderBottom(Border.NO_BORDER);
        //        if (col.NoRightBorder == false) cell.SetBorderRight(new SolidBorder(col.BorderWidth)); else cell.SetBorderRight(Border.NO_BORDER);
        //        if (col.NoTopBorder == false) cell.SetBorderTop(new SolidBorder(col.BorderWidth)); else cell.SetBorderTop(Border.NO_BORDER);
        //        if (col.NoLeftBorder == false) cell.SetBorderLeft(new SolidBorder(col.BorderWidth)); else cell.SetBorderLeft(Border.NO_BORDER);
        //    }
        //    cell.SetTextAlignment(TextAlignment.CENTER);
        //    cell.SetHorizontalAlignment(HorizontalAlignment.CENTER);
        //    cell.SetVerticalAlignment(VerticalAlignment.MIDDLE);
        //    if (col.IsItalic == true) cell.SetItalic();
        //    table.AddHeaderCell(cell);
        //}
        //private void AddDetailFooterColumn(iColumn col, Table table)
        //{
        //    string colText = "";
        //    if (col.MaxChar == 0)
        //        colText = col.Text;
        //    else
        //    {
        //        if (col.Text.Length > col.MaxChar)
        //        {
        //            colText = col.Text.Substring(0, col.MaxChar);
        //        }
        //        else
        //        {
        //            colText = col.Text;
        //        }
        //    }
        //    Cell cell = new Cell(col.RowSpan,col.ColSpan).Add(new Paragraph(colText==null?"":colText));
        //    cell.SetFont(GetPdfFont(col.FontName));
        //    cell.SetFontSize(col.FontSize + 4);
        //    cell.SetBold();
        //    if (col.NoBorder == true)
        //    {
        //        cell.SetBorder(Border.NO_BORDER);
        //    }
        //    else
        //    {
        //        cell.SetBorder(Border.NO_BORDER);
        //        if (col.NoBottomBorder == false) cell.SetBorderBottom(new SolidBorder(col.BorderWidth)); else cell.SetBorderBottom(Border.NO_BORDER);
        //        if (col.NoRightBorder == false) cell.SetBorderRight(new SolidBorder(col.BorderWidth)); else cell.SetBorderRight(Border.NO_BORDER);
        //        if (col.NoTopBorder == false) cell.SetBorderTop(new SolidBorder(col.BorderWidth)); else cell.SetBorderTop(Border.NO_BORDER);
        //        if (col.NoLeftBorder == false) cell.SetBorderLeft(new SolidBorder(col.BorderWidth)); else cell.SetBorderLeft(Border.NO_BORDER);
        //    }
        //    cell.SetTextAlignment(TextAlignment.CENTER);
        //    cell.SetHorizontalAlignment(HorizontalAlignment.CENTER);
        //    cell.SetVerticalAlignment(VerticalAlignment.MIDDLE);
        //    if (col.IsItalic == true) cell.SetItalic();
        //    table.AddFooterCell(cell);
        //}
        private void LoadCsvTable(string filename, ArrayList cols, Table table)
        {
            string csv = File.ReadAllText(filename);
            if (string.IsNullOrEmpty(csv) == true) return;
            string[,] darray = LoadCsv(csv);
            int rowNum = darray.GetLength(0);
            if (FixedRows > 0) rowNum = FixedRows;

            for (int r = 0; r < rowNum ; r++)
            {
                int c = 0;
                foreach (iColumn col in cols)
                {
                    if (c < darray.GetLength(1))
                    {
                        AddColumn(col, table, darray[r, c]);
                    }
                    else
                    {
                        AddColumn(col, table, "");
                    }
                    c++;
                }
            }
        }
        private void LoadBlankTable(ArrayList cols, Table table)
        {
            int rowNum = 0;
            if (FixedRows > 0) rowNum = FixedRows;
            for (int r = 0; r < rowNum; r++)
            {
                foreach (iColumn col in cols)
                {
                    iColumn iCol = col as iColumn;
                    AddColumn(iCol, table, "");
                }
            }
        }
        private void PrepareSqlReportData(Invoice inv, JObject param, List<iColumn> cols, Table table)
        {
            var constring = inv.Document.ConnectionString;
            var reportQuery = inv.Document.ReportSource;
            var detailQuery = inv.Document.DetailSource;

            using (SqlConnection con = new SqlConnection())
            {

                JObject reportData = null;
                if (string.IsNullOrEmpty(reportQuery) == false)
                {
                    reportData = con.Query<JObject>(reportQuery, param).FirstOrDefault();

                }
                if (string.IsNullOrEmpty(detailQuery) == false)
                {
                    List<JObject> detailData = con.Query<JObject>(detailQuery, param).ToList();

                }


            }
        }

        private void LoadSqlTable(List<IDictionary<string, object>> rows, string fields, List<iColumn> cols, Table table)
        {
            if (rows == null) return;
            if (fields == null) fields = "";
            string[] colFields = fields.Split(',');
            foreach (var row in rows)
            {
                int c = 0;
                foreach (iColumn col in cols)
                {
                    if (c < colFields.Length)
                    {
                        string coltext;
                        try
                        {
                            var txt = row[colFields[c]];
                            coltext = txt == null ? "" : row[colFields[c]].ToString();
                        }
                        catch
                        {
                            coltext = "";
                        }

                        AddColumn(col, table, coltext);
                    }
                    else
                    {
                        AddColumn(col, table, "");
                    }
                    c++;
                }
            }
        }

        private void LoadSqlTableFixedRows(List<IDictionary<string, object>> rows, string fields, ArrayList cols, Table table, int startRow)
        {
            if (rows == null) return;
            if (fields == null) fields = "";
            string[] colFields = fields.Split(',');
            int rowNum = rows.Count();
            if (FixedRows > 0) rowNum = FixedRows;
            for (int r = 0; r < rowNum; r++)
            {
                IDictionary<string, object> row = null;
                if (r < rows.Count())
                {
                    try
                    {
                        row = rows[r + startRow];
                    }
                    catch
                    {
                        row = null;
                    }
                }


                int c = 0;

                foreach (iColumn col in cols)
                {
                    if (row == null)
                    {
                        AddColumn(col, table, "");
                        continue;
                    }
                    if (c < colFields.Length)
                    {
                        string coltext;
                        try
                        {
                            var txt = row[colFields[c]];
                            coltext = txt == null ? "" : txt.ToString();
                        }
                        catch
                        {
                            coltext = "";
                        }

                        AddColumn(col, table, coltext);
                    }
                    else
                    {

                        AddColumn(col, table, " ");
                    }
                    c++;
                }
            }
        }

        private void AddTable(iTable colTable, Table parentTable)
        {
            float[] cols = colTable.Columns.ToArray().Select(x => { float i = 1; return i; }).ToArray();
            Table table = CreateTable(colTable, cols);
            foreach (var col in colTable.Columns)
            {
                if (col.GetType().Equals(typeof(iColumn)))
                {
                    AddColumn((iColumn)col, table);
                }
                if (col.GetType().Equals(typeof(iTable)))
                {
                    AddTable((iTable)col, table);
                }
            }
            Cell cell = new Cell(colTable.RowSpan, colTable.ColSpan);
            if (colTable.NoBorder == true)
            {
                cell.SetBorder(Border.NO_BORDER);
            }
            else
            {
                table.SetBorder(Border.NO_BORDER);
                if (colTable.NoBottomBorder == false) cell.SetBorderBottom(new SolidBorder(colTable.BorderWidth)); else table.SetBorderBottom(Border.NO_BORDER);
                if (colTable.NoRightBorder == false) cell.SetBorderRight(new SolidBorder(colTable.BorderWidth)); else table.SetBorderRight(Border.NO_BORDER);
                if (colTable.NoTopBorder == false) cell.SetBorderTop(new SolidBorder(colTable.BorderWidth)); else table.SetBorderTop(Border.NO_BORDER);
                if (colTable.NoLeftBorder == false) cell.SetBorderLeft(new SolidBorder(colTable.BorderWidth)); else table.SetBorderLeft(Border.NO_BORDER);
            }
            SetCellMargins(cell, colTable.Margin);
            SetCellPaddings(cell, colTable.Padding);
            if (colTable.Height > 0)
            {
                cell.SetHeight(colTable.Height);
            }
            cell.Add(table);
            parentTable.AddCell(cell);
        }
        private void AddColumn(iColumn col, Table table, string text = null, TableType tableType = TableType.table)
        {
            Cell cell = new Cell(col.RowSpan, col.ColSpan);

            string colText = "";

            if (text != null)
            {
                if (col.MaxChar == 0)

                    colText = text;


                else
                {
                    if (text.Length > col.MaxChar)
                    {
                        colText = text.Substring(0, col.MaxChar);
                    }
                    else
                    {
                        colText = text;
                    }
                }
            }
            else
            {
                string txtCol = GetColumnText(col);
                if (col.MaxChar == 0)
                    colText = txtCol;
                else
                {
                    if (col.Text.Length > col.MaxChar)
                    {
                        colText = txtCol.Substring(0, col.MaxChar);
                    }
                    else
                    {
                        colText = txtCol;
                    }
                }
            }
            if (string.IsNullOrEmpty(col.FormatString) == false)
            {
                if (decimal.TryParse(colText, out decimal dec) == true)
                {
                    colText = dec.ToString(col.FormatString);
                }
            }

            if (text != null && decimal.TryParse(colText, out decimal numtext))
            {
                cell.SetNextRenderer(new TruncateCellRenderer(cell, colText));
            }

            else
            {
                cell.Add(new Paragraph(colText == null ? "" : colText));
            }



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
            cell.SetVerticalAlignment(GetVerticalAlignment(col.VerticalAlignment));
            SetCellMargins(cell, col.Margin);
            SetCellPaddings(cell, col.Padding);

            if (col.MinHeight > 0)
            {
                cell.SetMinHeight(col.MinHeight);
            }
            if (col.Height > 0)
            {
                cell.SetHeight(col.Height);
            }
            if (col.IsItalic == true) cell.SetItalic();
            if (tableType == TableType.header)
            {
                table.AddHeaderCell(cell);
            }
            else if (tableType == TableType.footer)
            {
                table.AddFooterCell(cell);
            }
            else
            {
                table.AddCell(cell);
            }

        }
        private string GetColumnText(iColumn col)
        {
            if (string.IsNullOrEmpty(col.Text) == true) return "";
            if (col.DisplayOnlyinLastPage == true)
            {
                if (IsLastPage == false)
                {
                    return "";
                }
            }
            //get the value from the ReportSouce queried 
            if (col.Text.Substring(0, 1) == "@")
            {
                if (col.Text.Length == 1) return col.Text;
                var key = col.Text.Substring(1);
                try
                {
                    var txt = this.ReportData[key];
                    return txt == null ? col.Text : txt.ToString() == "" ? col.Text : txt.ToString();
                }
                catch
                {
                    return col.Text;
                }


            }
            else if (col.Text.Substring(0, 1) == "$")//get the value from parameter
            {
                if (col.Text.Length == 1) return col.Text;
                var key = col.Text.Substring(1);

                try
                {
                    var txt = this.InputParameters[key];
                    return txt == null ? col.Text : txt.ToString() == "" ? col.Text : txt.ToString();
                }
                catch
                {
                    return col.Text;
                }

            }
            else
            {
                return col.Text;
            }
        }
        private UnitValue[] GetUnitValue(iColumnArrayUnit arrayUnit, float[] cols)
        {
            switch (arrayUnit)
            {
                case iColumnArrayUnit.PercentArray:
                    return UnitValue.CreatePercentArray(cols);
                case iColumnArrayUnit.PointArray:
                    return UnitValue.CreatePointArray(cols);
                default:
                    return UnitValue.CreatePointArray(cols);
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
                    return HorizontalAlignment.RIGHT;
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
        public PdfFont GetPdfFont(iFonts fontname)
        {

            switch (fontname)
            {
                case iFonts.HELVETICA:
                    return PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                case iFonts.HELVETICA_BOLD:
                    return PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                case iFonts.TIMES_ROMAN:
                    return PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
                case iFonts.COURIER:
                    return PdfFontFactory.CreateFont(StandardFonts.COURIER);
                case iFonts.COURIER_BOLD:
                    return PdfFontFactory.CreateFont(StandardFonts.COURIER_BOLD);
                case iFonts.COURIER_BOLDOBLIQUE:
                    return PdfFontFactory.CreateFont(StandardFonts.COURIER_BOLDOBLIQUE);
                case iFonts.TIMES_BOLDITALIC:
                    return PdfFontFactory.CreateFont(StandardFonts.TIMES_BOLDITALIC);
                case iFonts.TIMES_ITALIC:
                    return PdfFontFactory.CreateFont(StandardFonts.TIMES_ITALIC);
                case iFonts.SYMBOL:
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

        private string[,] LoadCsv(string whole_file)
        {

            // Split into lines.
            whole_file = whole_file.Replace('\n', '\r');
            string[] lines = whole_file.Split(new char[] { '\r' },
                StringSplitOptions.RemoveEmptyEntries);

            // See how many rows and columns there are.
            int num_rows = lines.Length;
            int num_cols = lines[0].Split(',').Length;

            // Allocate the data array.
            string[,] values = new string[num_rows, num_cols];

            // Load the array.
            for (int r = 0; r < num_rows; r++)
            {
                string[] line_r = lines[r].Split(',');
                for (int c = 0; c < num_cols; c++)
                {
                    values[r, c] = line_r[c];
                }
            }

            // Return the values.
            return values;
        }

        //Loading json to c# class
        public Invoice LoadInvFromJson(string jsonString)
        {
            var jObjectFromjson = JsonConvert.DeserializeObject<JObject>(jsonString);

            var invoceFromjson = JsonConvert.DeserializeObject<Invoice>(jsonString);
            if (invoceFromjson.Document == null) throw new Exception("File format is not correct. It cannot be open");
            Invoice inv = invoceFromjson.DeepCopy();
            var jdetail = jObjectFromjson.GetValue("Detail");
            if (jdetail != null)
            {
                var JdetailDetail = ((JObject)jdetail).GetValue("Detail");
                if (JdetailDetail != null)
                {
                    var jdetailcols = ((JObject)JdetailDetail).GetValue("Columns");
                    var detailCols = jdetailcols.ToObject<List<iColumn>>();
                    inv.Detail.Detail.Columns = new ArrayList(detailCols);
                }
            }
            //Loading in Document Clas
            inv.Document = invoceFromjson.Document.DeepCopy();
            var jDocument = jObjectFromjson.GetValue("Document") as JObject;
            if (jDocument != null)
            {
                var detailsourse = jDocument.GetValue("DetailSource");
                if (detailsourse != null)
                {
                    inv.Document.setDetailSource(detailsourse != null ? detailsourse.ToString() : "");
                }
                var reportSource = jDocument.GetValue("ReportSource");
                inv.Document.setReportSource(reportSource == null ? "" : reportSource.ToString());
                var constring = jDocument.GetValue("ConnectionString");
                inv.Document.setSqlConnection(constring == null ? "" : constring.ToString());
                var QueryParameter = jDocument.GetValue("QueryParameter");
                inv.Document.setQueryParameter(QueryParameter == null ? "" : QueryParameter.ToString());

            }

            //Loading reportheaders
            ArrayList rptHeaders = ConvertToObjectListFromJson(inv.ReportHeaders);
            inv.SetReportHeaders(rptHeaders);

            //foreach (var rptheader in invoceFromjson.ReportHeaders)
            //{
            //    if (rptheader != null && rptheader.GetType().Equals(typeof(JObject)))
            //    {
            //        AddColumnToTableFromJson((JObject)rptheader, inv.ReportHeaders);
            //    }

            //}
            //Loading Detail
            //inv.Detail.ColumnArrayUnit = invoceFromjson.Detail.ColumnArrayUnit;
            //inv.Detail.DetailHeader.ColumnArrayUnit = invoceFromjson.Detail.DetailHeader.ColumnArrayUnit;

            ArrayList DetailHeaderColumns = ConvertToObjectListFromJson(inv.Detail.DetailHeader.Columns);
            inv.Detail.DetailHeader.Columns = DetailHeaderColumns;
            ArrayList DetailFooterColumns = ConvertToObjectListFromJson(inv.Detail.DetailFooter.Columns);
            inv.Detail.DetailFooter.Columns = DetailFooterColumns;
            //Loadint Report Footers
            ArrayList rptFooters = ConvertToObjectListFromJson(inv.ReportFooters);
            inv.SetReportFooters(rptFooters);
            //foreach (var col in invoceFromjson.Detail.DetailHeader.Columns)
            //{
            //    if (col != null && col.GetType().Equals(typeof(JObject)))
            //    {
            //        AddColumnToTableFromJson((JObject)col, inv.Detail.DetailHeader.Columns);
            //    }
            //}
            //foreach (var col in invoceFromjson.Detail.DetailFooter.Columns)
            //{
            //    if (col != null && col.GetType().Equals(typeof(JObject)))
            //    {
            //        AddColumnToTableFromJson((JObject)col, inv.Detail.DetailFooter.Columns);
            //    }
            //}
            //foreach (var rptfooter in invoceFromjson.ReportFooters)
            //{
            //    if (rptfooter != null && rptfooter.GetType().Equals(typeof(JObject)))
            //    {
            //        AddColumnToTableFromJson((JObject)rptfooter, inv.ReportFooters);
            //    }

            //}
            return inv;
        }
        private void AddColumnToTableFromJson(JObject obj, ArrayList Columns)
        {
            iTable tbl = ((JObject)obj).ToObject<iTable>();
            if (tbl.Columns.Count > 0)
            {
                iTable newTable = tbl.DeepCopy();
                Columns.Add(newTable);
                newTable.ColumnArrayUnit = tbl.ColumnArrayUnit;
                newTable.Columns = new ArrayList();
                foreach (var col in tbl.Columns)
                {
                    JObject column = (JObject)col;
                    AddColumnToTableFromJson(column, newTable.Columns);
                }
            }
            else
            {
                iColumn column = ((JObject)obj).ToObject<iColumn>();
                if (column != null)
                {
                    Columns.Add(column);
                }

            }
        }
        private ArrayList ConvertToObjectListFromJson(ArrayList arrayList)
        {
            ArrayList Columns = new ArrayList();
            foreach (var obj in arrayList)
            {
                iTable tbl = ((JObject)obj).ToObject<iTable>();
                if (tbl.Columns.Count > 0)
                {
                    iTable newTable = tbl.DeepCopy();
                    Columns.Add(newTable);
                    newTable.ColumnArrayUnit = tbl.ColumnArrayUnit;
                    newTable.Columns = new ArrayList();
                    //foreach (var col in tbl.Columns)
                    //{
                    //    JObject column = (JObject)col;
                    newTable.Columns = ConvertToObjectListFromJson(tbl.Columns);
                    //}
                }
                else
                {
                    iColumn column = ((JObject)obj).ToObject<iColumn>();
                    if (column != null)
                    {
                        Columns.Add(column);
                    }

                }
            }
            return Columns;
        }

        private JObject prepareParameter(string jsonString)
        {
            //ExpandoObject obj = JsonConvert.DeserializeObject<ExpandoObject>(jsonString);
            JObject jobj = JsonConvert.DeserializeObject<JObject>(jsonString);
            return jobj;
        }

        public void SetInputParameter(string jsonString)
        {
            _InputParameter = jsonString;
        }

        public static float MillimetersToPoints(float value)
        {
            return (value / 25.4f) * 72f;
        }
    }
}

