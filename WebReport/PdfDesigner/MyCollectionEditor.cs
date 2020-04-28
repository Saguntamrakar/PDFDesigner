using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Design;
using System.Windows.Forms;

namespace PdfDesigner
{
    public class MyCollectionEditor: CollectionEditor
    {
        public MyCollectionEditor(Type type)
        : base(type)
        {
        }

        protected override CollectionForm CreateCollectionForm()
        {
            CollectionForm form = base.CreateCollectionForm();
            var addButton = (ButtonBase)form.Controls.Find("addButton", true).First();
            addButton.Click += (sender, e) =>
            {
                MessageBox.Show("hello world");
            };
            return form;
        }
    }
}
