using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PdfDesigner
{
    public partial class SqlConnectionDialog : Form
    {
        public string ConnectionString { get; set; }
        public string ReportQuery { get; set; }
        public string DetailQuery { get; set; }
        public string Parameter { get; set; }
        public SqlConnectionDialog()
        {
            InitializeComponent();
            txtConnectionString.DataBindings.Add("Text", this, "ConnectionString");
            txtReportQuery.DataBindings.Add("Text", this, "ReportQuery");
            txtDetailQuery.DataBindings.Add("Text", this, "DetailQuery");
            txtParameter.DataBindings.Add("Text", this, "Parameter");
        }

        private void SqlConnectionDialog_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
