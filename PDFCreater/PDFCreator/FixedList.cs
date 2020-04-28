using PDfCreator.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PDFCreator
{
    public class FixedList : IEnumerable<iColumn>
    {
        public IEnumerator<iColumn> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
