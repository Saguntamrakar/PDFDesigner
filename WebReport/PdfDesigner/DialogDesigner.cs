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
    public partial class DialogDesigner : Form
    {
        public List<DlgParameter> DialogParamList { get; set; } = new List<DlgParameter>();
        public List<DlgParameterControl> DialogParamControl { get; set; } = new List<DlgParameterControl>();
        private string[] param;
        public DialogDesigner()
        {
            InitializeComponent();
        }

        public void LoadParameterDialogs(string paramString, IDictionary<string, object> parameters)
        {
            if (string.IsNullOrEmpty(paramString) == true) return;
            param = paramString.Split(',');
            listBox1.Items.Clear();
            listBox1.DisplayMember = "name";
            foreach (var p in param)
            {
                // DialogParamList.Add(new DlgParameter { name = p });
                var par = DialogParamList.Find(x => x.name == p);
                if (par == null)
                {
                    listBox1.Items.Add(new DlgParameterControl
                    {
                        name = p,

                    });
                }
                else
                {
                    listBox1.Items.Add(new DlgParameterControl { 
                    name=par.name,cssClass=par.cssClass,disabled=par.disabled,inputid=par.inputid,label=par.label,
                    noSearch=par.noSearch,optionQuery=par.optionQuery,options=par.options.ToList(),placeholder=par.placeholder,
                    value=par.value,width=par.width,controlType=getControlTypeEnum(par.type)
                    });

                }



            }

        }

        private ControlTypeEnum getControlTypeEnum(string enumName)
        {
            var e = (ControlTypeEnum)Enum.Parse(typeof(ControlTypeEnum), enumName);
            return e;
        }

        private void Btn_Ok_Click(object sender, EventArgs e)
        {
            DialogParamList.Clear();
            foreach (DlgParameterControl par in listBox1.Items)
            {
                var dlg = new DlgParameter();

                dlg.name = par.name;
                    dlg.cssClass = par.cssClass;
                dlg.disabled = par.disabled;
                dlg.inputid = par.inputid;
                dlg.label = par.label;
                dlg.noSearch = par.noSearch;
                dlg.optionQuery = par.optionQuery;
                dlg.options = par.options.ToList();
                dlg.placeholder = par.placeholder;
                dlg.value = par.value;
                dlg.width = par.width;
                dlg.type = Enum.GetName(typeof(ControlTypeEnum), par.controlType);
                DialogParamList.Add(dlg);
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void ListItem_click(object sender, MouseEventArgs e)
        {

            if (sender == null) return;
            var obj = sender as ListBox;


            propertyGrid1.SelectedObject = obj.SelectedItem;

        }
    }
}

