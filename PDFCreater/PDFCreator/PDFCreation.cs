using PDfCreator.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace PDfCreator
{
    public class Invoice
    {
        private ArrayList _Reportheaders;
        private ArrayList _ReportFooters;
        private iDetail _Detail;
        //private string _csvData;
        
        public iDocument Document { get; set; }
        
        public ArrayList ReportHeaders { get { return _Reportheaders; } }
        public ArrayList ReportFooters { get { return _ReportFooters; } }
        public iDetail Detail { get { return _Detail; } }
        //public string DetailData { get { return _csvData; } }
       
        public Invoice()
        {
            Document = new iDocument();
            _Reportheaders = new ArrayList();
            _ReportFooters = new ArrayList();
            _Detail = new iDetail();
        }

        public iTable NewTable(ArrayList parent) 
        {

            //var header = (T)Activator.CreateInstance(typeof(T));
            var tbl = new iTable();
                parent.Add(tbl);
            return tbl;
            

        }

        public iColumn  NewColumn(iTable header)
        {
            iColumn column = new iColumn();
            column.Text = "New Column";
            header.Columns.Add(column);
            return  column;
        }
        public void NewTableColumn(iTable header)
        {
            iTable column = new iTable();
            header.Columns.Add(column);
        }
        public bool RemoveColumn(iTable header,iColumn col)
        {
            var colCount = header.Columns.Count;
            header.Columns.Remove(col);
            if (header.Columns.Count == colCount - 1) return true;
            return false;
        }
        public void RemoveTableColumn(iTable header, iTable col)
        {
            header.Columns.Remove(col);
        }
        public bool  RemoveTable(iTable table,object obj)
        {
            if (obj.GetType().Equals(typeof(ArrayList)))
            {
                ArrayList lst = obj as ArrayList;
                var lstCount = lst.Count;
                lst.Remove(table);
                if(lst.Count == (lstCount - 1)){
                    return true;
                }
            }
            return false;
        }

        public void AddDetailcsvData(string fileName)
        {
            Document.setDetailSource( fileName);
        }
        //public void AddReportcsvData( string fileName)
        //{
        //    Document.setReportCsvSource(fileName);
        //}
        public void AddSQlConfiguration(String connetionstring,string reportquery,string detailquery,string parameters)
        {
            Document.setSqlConnection ( connetionstring);
            Document.setDetailSource ( detailquery);
            Document.setReportSource ( reportquery);
            Document.setQueryParameter(parameters);
        }
        public Invoice DeepCopy()
        {
            Invoice inv = (Invoice)this.MemberwiseClone();
            return inv;
        }

        public void SetReportHeaders(ArrayList rptHeaders)
        {
            _Reportheaders = rptHeaders;
        }

        public void SetReportFooters(ArrayList rptFooters)
        {
            _ReportFooters = rptFooters;
        }
    }
}

