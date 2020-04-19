using System;
using System.Collections.Generic;
using System.Text;

namespace PDfConsole
{
    public class Invoice
    {
        private List<iHeader> _headers;
        public iDocument Document { get; set; }
        public List<iHeader> Headers { get { return _headers; } }
        public Invoice()
        {
            _headers = new List<iHeader>();
        }

        public void NewHeader()
        {
            iHeader header = new iHeader();
            _headers.Add(header);

        }

        public void NewColumn(iHeader header)
        {
            iHeaderColumn column = new iHeaderColumn();
            header.Columns.Add(column);
        }

        public void RemoveColumn(iHeader header,iHeaderColumn col)
        {
            header.Columns.Remove(col);
        }

        public void RemoveHeader(iHeader header)
        {
            Headers.Remove(header);
        }
    }
}

