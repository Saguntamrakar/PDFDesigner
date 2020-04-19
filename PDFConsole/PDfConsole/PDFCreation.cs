using System;
using System.Collections.Generic;
using System.Text;

namespace PDfConsole
{
    public class Invoice
    {
        private List<iHeader> _headers;
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
    }
}

