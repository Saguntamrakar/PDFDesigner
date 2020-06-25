using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PDFDesignerCore
{
    public partial class ParameterDialog : Form
    {
        private string[] param; 
        public string jsonParam { get; set; }
        public ParameterDialog()
        {
            InitializeComponent();
        }

        private void ParameterDialog_Load(object sender, EventArgs e)
        {

        }
        public void  LoadParameters(string paramString,IDictionary<string,object> parameters)
        {
            if (string.IsNullOrEmpty(paramString) == true) return;
            param = paramString.Split(',');
            int pointX = 30;
            int pointY = 40;
            panel1.Controls.Clear();
            foreach (var p in param)
            {
                Label lbl = new Label();
                lbl.Text = p;
                lbl.Location = new Point(pointX, pointY);
                TextBox tbx = new TextBox();
                tbx.Name = p;
                tbx.Location = new Point(pointX + 100, pointY);
                panel1.Controls.Add(lbl);
                panel1.Controls.Add(tbx);
                pointX = 30;
                pointY = pointY + 20;
                try
                {
                    var txt = parameters[p];
                    tbx.Text = txt.ToString();
                }
                catch
                {

                }
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            jsonParam = "{" + string.Join(",", param.Select(x => { var s = $"\"{x}\":\"{panel1.Controls[x].Text}\""; return s; })) +"}";
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
