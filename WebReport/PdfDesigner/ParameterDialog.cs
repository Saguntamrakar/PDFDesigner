using PDfCreator.Models;
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
    public partial class ParameterDialog : Form
    {
        private string[] param;
        public string jsonParam { get; set; }
        public Dictionary<string,object> DictParam { get; set; }
        public List<DlgParameter> DialogParamList { get; set; } = new List<DlgParameter>();
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
                //ComboBox cmb = new ComboBox();
                //cmb.Items.Add("String");cmb.Items.Add("Date");
                //var cmbValue = DialogParamList.Find(x => x.name == p);
                //if(cmbValue != null)
                //{
                //    cmb.SelectedItem = cmbValue.type;
                //}
                //else
                //{
                //    cmb.SelectedValue = "String";
                //}
                //cmb.Location = new Point(tbx.Location.X + 100, pointY);
                //cmb.Name = p + "_cmb";
                panel1.Controls.Add(lbl);
                panel1.Controls.Add(tbx);
                //panel1.Controls.Add(cmb);
                pointX = 30;
                pointY = pointY + 20;
                try
                {
                    if (parameters != null)
                    {
                        var txt = parameters[p] == null ? "" : parameters[p];
                        tbx.Text = txt.ToString();
                    }
                    else
                    {
                        tbx.Text = "";
                    }
                }
                catch
                {

                }
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            jsonParam = "{" + string.Join(",", param.Select(x => { var s = $"\"{x}\":\"{panel1.Controls[x].Text}\""; return s; })) +"}";
            DictParam = param.Select(x =>  new KeyValuePair<string, object>(x, panel1.Controls[x].Text)).ToDictionary(x=>x.Key,x=>x.Value);
            //var dialoglist = param.Select(x => new DlgParameter { name = x, label = x, type = panel1.Controls[x + "_cmb"].Text }).ToList();
            //DialogParamList = dialoglist;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
