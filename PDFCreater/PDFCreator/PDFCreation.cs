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

        public void NewTable(ArrayList parent) 
        {

            //var header = (T)Activator.CreateInstance(typeof(T));
            var tbl = new iTable();
                parent.Add(tbl);
            
            

        }

        public void NewColumn(iTable header)
        {
            iColumn column = new iColumn();
            header.Columns.Add(column);
        }
        public void NewTableColumn(iTable header)
        {
            iTable column = new iTable();
            header.Columns.Add(column);
        }
        public void RemoveColumn(iTable header,iColumn col)
        {
            header.Columns.Remove(col);
        }
        public void RemoveTableColumn(iTable header, iTable col)
        {
            header.Columns.Remove(col);
        }
        public void RemoveTable(iTable table)
        {
            ReportHeaders.Remove(table);
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
    }
}

