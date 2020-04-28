using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PDfCreator;
using System.Reflection;
using PDfCreator.Print;
using PDfCreator.Models;
using Dapper;
namespace PdfDesigner
{
    public partial class Form1 : Form
    {
        Invoice inv;
        ContextMenuStrip myContextMenuStrip;
        InvoicePrinting inoicePrinting;
        public Form1()
        {
            InitializeComponent();
            CreateContextMenu();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            inv = new Invoice();
            inoicePrinting = new InvoicePrinting();
        }
        private void LoadTree()
        {
            treeView1.Nodes.Clear();
            var doc = treeView1.Nodes.Add("Document");
            doc.Tag = inv.Document;
            //Report Header Section
            var headernod = treeView1.Nodes.Add("Headers");
            headernod.Tag = inv.ReportHeaders;
            int i = 0;
            foreach (var head in inv.ReportHeaders)
            {
                i++;
                var header = headernod.Nodes.Add($"Header({i})");
                header.Tag = head;
                if (head.GetType().Equals(typeof(iTable)))
                {
                    foreach (var col in ((iTable)head).Columns)
                    {
                        LoadNode(col, header);
                    }
                }

            }

            //Report Detail Section
            TreeNode detail = treeView1.Nodes.Add("Detail");
            detail.Tag = inv.Detail;

            TreeNode detailHeader = detail.Nodes.Add("Detail Header");
            detailHeader.Tag = inv.Detail.DetailHeader;
            foreach (var col in inv.Detail.DetailHeader.Columns)
            {
                string colName = col.GetType().Equals(typeof(iColumn)) ? ((iColumn)col).Text : "Table";
                var column = detailHeader.Nodes.Add(colName);
                column.Tag = col;
            }
            TreeNode detailSection = detail.Nodes.Add("Data");
            detailSection.Tag = inv.Detail.Detail;
            string[] detailColNames = new string[] { "" };
            if (string.IsNullOrEmpty(inv.Document.DetailFields) == false)
            {
                detailColNames = inv.Document.DetailFields.Split(',');
            }
            int c = 0;
            if (inv.Detail.Detail != null)
            {
                foreach (iColumn col in inv.Detail.Detail.Columns)
                {
                    string colName = "";
                    if (c < detailColNames.Length)
                    {
                        colName = detailColNames[c];
                    }
                    var colnod = detailSection.Nodes.Add(string.IsNullOrEmpty(colName) == null ? "Column(" + c + ")" : colName);
                    col.Text = colName;

                    colnod.Tag = col;
                    c++;
                }
            }

            TreeNode footerSection = detail.Nodes.Add("Detail Footer");
            footerSection.Tag = inv.Detail.DetailFooter;
            foreach (var col in inv.Detail.DetailFooter.Columns)
            {
                string colName = col.GetType().Equals(typeof(iColumn)) ? ((iColumn)col).Text : "Table";
                var column = footerSection.Nodes.Add(colName);
                column.Tag = col;
            }

            //Report Footer Section
            var footerernod = treeView1.Nodes.Add("Footers");
            footerernod.Tag = inv.ReportFooters;
            i = 0;
            foreach (var foot in inv.ReportFooters)
            {
                i++;
                var footer = footerernod.Nodes.Add($"Footer({i})");
                footer.Tag = foot;
                if (foot.GetType().Equals(typeof(iTable)))
                {
                    foreach (var col in ((iTable)foot).Columns)
                    {
                        LoadNode(col, footer);
                    }
                }

            }
        }

        private void LoadNode(object col, TreeNode parentNode)
        {
            if (col.GetType().Equals(typeof(iColumn)))
            {
                var nod = parentNode.Nodes.Add(((iColumn)col).Text);
                nod.Tag = (iColumn)col;
            }
            else if (col.GetType().Equals(typeof(iTable)))
            {
                var nod = parentNode.Nodes.Add("Table");
                nod.Tag = col;
                foreach (var columns in ((iTable)col).Columns)
                {
                    LoadNode(columns, nod);
                }
            }

        }
        private void LoadPdf(string ExportFileName = "")
        {
            try
            {
                string DEST = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test1.pdf");
                if (string.IsNullOrEmpty(ExportFileName))
                {
                    if (Directory.Exists(ExportFileName) == true)
                    {
                        DEST = ExportFileName;
                    }
                }
                FileInfo file = new FileInfo(DEST);
                string jsonParam = "";
                IDictionary<string, object> dictParam = new Dictionary<string, object>();

                if (string.IsNullOrEmpty(inv.Document.QueryParameter) == false)
                {
                    string paramString = inv.Document.QueryParameter;
                    if (inoicePrinting.DetailData == null || inoicePrinting.DetailData.Count() == 0)
                    {
                        jsonParam = OpenParameter_Dialog(paramString);
                        var jobjParam = JsonConvert.DeserializeObject<JObject>(jsonParam.ToUpper());
                        dictParam = jobjParam.ToObject<Dictionary<string, object>>();
                        inoicePrinting.InputParameters = dictParam;
                        PrepareSqlReportData(inoicePrinting, inv, dictParam);
                    }


                }

                using(MemoryStream memoryStream = new MemoryStream())
                {
                    inoicePrinting.PrintInvoice(inv, DEST,memoryStream);

                    Stream stream = new MemoryStream(memoryStream.ToArray());
                    pdfDocumentViewer1.LoadFromStream(stream);
                    //pdfDocumentViewer1.LoadFromFile(DEST);
                }
                

               
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }
        private void PrepareSqlReportData(InvoicePrinting invPrint, Invoice inv, IDictionary<string, object> param)
        {
            var constring = inv.Document.ConnectionString;
            var reportQuery = inv.Document.ReportSource;
            var detailQuery = inv.Document.DetailSource;

            using (SqlConnection con = new SqlConnection(constring))
            {

                IDictionary<string, object> reportData = null;
                if (string.IsNullOrEmpty(reportQuery) == false)
                {
                    reportData = (IDictionary<string, object>)con.Query(reportQuery, param).FirstOrDefault();
                    invPrint.ReportData = reportData;
                }
                if (string.IsNullOrEmpty(detailQuery) == false)
                {
                    List<IDictionary<string, object>> detailData = con.Query(detailQuery, param).Select(row => (IDictionary<string, object>)row).ToList();
                    invPrint.DetailData = detailData;
                }


            }
        }

        private string OpenParameter_Dialog(string paramString)
        {
            if (string.IsNullOrEmpty(paramString)) return "";
            try
            {
                ParameterDialog pmDlg = new ParameterDialog();
                pmDlg.LoadParameters(paramString, inoicePrinting.InputParameters);
                if (pmDlg.ShowDialog() == DialogResult.OK)
                {
                    string jsonParam = pmDlg.jsonParam;
                    return jsonParam;
                }
                return "";
                //else
                //{
                //    string[] par = paramString.Split(',');
                //    string jsonParam = "{" + string.Join(",", par.Select(x => { var s = $"\"{x}:\"\""; return s; })) + "}";
                //    return jsonParam;
                //}
            }
            catch (Exception)
            {

                throw;
            }

        }
        private void button1_Click(object sender, EventArgs e)
        {
            LoadPdf();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            LoadPdf();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            LoadPdf();
        }

        private void LoadPropertyGrid(object obj)
        {
            if (obj == null) return;
            if (obj.GetType().Equals(typeof(iColumn)))
            {
                propertyGrid1.SelectedObject = (iColumn)obj;
            }
            else if (obj.GetType().Equals(typeof(iTable)))
            {
                propertyGrid1.SelectedObject = (iTable)obj;
            }
            else
            {
                propertyGrid1.SelectedObject = obj;
            }

        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode nod = e.Node;
            if (nod.Tag == null) { LoadPropertyGrid(null); return; }
            if (nod.Tag.GetType().Equals(typeof(iColumn)))
            {
                LoadPropertyGrid(nod.Tag);
                return;
            }
            if (nod.Tag.GetType().Equals(typeof(iDocument)) || nod.Tag.GetType().Equals(typeof(iTable)) || nod.Tag.GetType().Equals(typeof(iDetail)) || nod.Tag.GetType().Equals(typeof(iFixedColTable)))
            {
                LoadPropertyGrid(nod.Tag);
            }



        }

        private void button1_Click_2(object sender, EventArgs e)
        {
            LoadPdf();
        }
        void treeView1MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // Select the clicked node
                treeView1.SelectedNode = treeView1.GetNodeAt(e.X, e.Y);
                if (treeView1.SelectedNode == null) return;
                if (treeView1.SelectedNode.Text == "Document")
                {
                    CreateContextDataMenu();
                    treeView1.ContextMenuStrip = myContextMenuStrip;
                    myContextMenuStrip.Show();
                    return;
                }
                if (treeView1.SelectedNode.Text == "Headers" || treeView1.SelectedNode.Text == "Footers")
                {
                    CreateContextMenu();
                    ShowContextMenuAddHeader(new string[] { "AddTable", "AddCellColumn" });
                    treeView1.ContextMenuStrip = myContextMenuStrip;
                    myContextMenuStrip.Show(treeView1, e.Location);
                    return;
                }
                CreateContextMenu();
                var tag = treeView1.SelectedNode.Tag;
                if (tag != null)
                {
                    var parentNod = treeView1.SelectedNode.Parent;
                    var parentTag = parentNod == null ? null : parentNod.Tag;
                    if (parentTag != null && parentTag.GetType().Equals(typeof(iTable)))
                    {

                    }
                    if (tag.GetType().Equals(typeof(iTable)))
                    {
                        if (treeView1.SelectedNode.Text == "Data" || treeView1.SelectedNode.Text=="Detail Header" || treeView1.SelectedNode.Text == "Detail Footer")
                        {
                            ShowContextMenuAddHeader(new string[] { "AddCellColumn" });
                        }
                        else
                        {
                            ShowContextMenuAddHeader(new string[] { "AddCellColumn", "AddTable", "RemoveTable" });
                        }
                        myContextMenuStrip.Show(treeView1, e.Location);
                    }
                    //if (tag.GetType().Equals(typeof(iTable)) && treeView1.SelectedNode.Text == "Detail")
                    //{
                    //    ShowContextMenuAddHeader(new string[] { "AddCellColumn" });
                    //    myContextMenuStrip.Show(treeView1, e.Location);
                    //}
                    //if (tag.GetType().Equals(typeof(iTable)) && treeView1.SelectedNode.Text == "Detail")
                    //{
                    //    ShowContextMenuAddHeader(new string[] { "AddCellColumn" });
                    //    myContextMenuStrip.Show(treeView1, e.Location);
                    //}
                    if (tag.GetType().Equals(typeof(iColumn)))
                    {
                        ShowContextMenuAddHeader(new string[] { "RemoveCellColumn" });
                        myContextMenuStrip.Show(treeView1, e.Location);
                    }
                    //if (tag.GetType().Equals(typeof(itablec)))
                    //{
                    //    ShowContextMenuAddHeader(new string[] { "RemoveTableColumn" });
                    //    myContextMenuStrip.Show(treeView1, e.Location);
                    //}
                    return;
                }

            }
        }

        void menuItem_Click(object sender, EventArgs e)

        {
            TreeNode selectedNod = treeView1.SelectedNode;
            var tag = selectedNod.Tag;
            ToolStripItem menuItem = (ToolStripItem)sender;

            if (menuItem.Name == "AddTable")

            {
                if (tag != null)
                {
                    if (tag.GetType().Equals(typeof(ArrayList)))
                    {
                        var parent = (ArrayList)tag;
                        var newtable = inv.NewTable(parent);
                        var newnod = selectedNod.Nodes.Add("Table");
                        newnod.Tag = newtable;
                        treeView1.SelectedNode = newnod;
                        newnod.EnsureVisible();
                        treeView1_NodeMouseClick(treeView1, new TreeNodeMouseClickEventArgs(newnod, MouseButtons.Left, 1, 0, 0));
                        return;
                    }
                    if (tag.GetType().Equals(typeof(iTable)))
                    {
                        var parent = (iTable)tag;
                        var newtable = inv.NewTable(parent.Columns);
                        var newnod = selectedNod.Nodes.Add("Table");
                        newnod.Tag = newtable;
                        treeView1.SelectedNode = newnod;
                        newnod.EnsureVisible();
                        treeView1_NodeMouseClick(treeView1, new TreeNodeMouseClickEventArgs(newnod, MouseButtons.Left, 1, 0, 0));
                        return;
                    }

                }


            }
            if (menuItem.Name == "AddCellColumn")
            {
                iTable header = treeView1.SelectedNode.Tag as iTable;
                if (header != null)
                {
                    var col = inv.NewColumn(header);
                    var newnod = selectedNod.Nodes.Add(col.Text);
                    newnod.Tag = col;
                    treeView1.SelectedNode = newnod;
                    newnod.EnsureVisible();
                    treeView1_NodeMouseClick(treeView1, new TreeNodeMouseClickEventArgs(newnod, MouseButtons.Left, 1, 0, 0));
                    //LoadTree();
                }


            }
            if (menuItem.Name == "RemoveTable")
            {
                var currenNod = treeView1.SelectedNode;
                iTable header = currenNod.Tag as iTable;
                var parentNod = currenNod.Parent;
                var parent = parentNod.Tag;
                if (header != null)
                {
                    if (parent.GetType().Equals(typeof(iTable)))
                    {
                        if (inv.RemoveTable(header, ((iTable)parent).Columns) == true)
                        {
                            currenNod.Remove();
                        }
                    }
                    else
                    {
                        if (inv.RemoveTable(header, parent) == true)
                        {
                            currenNod.Remove();
                        }
                    }

                    //LoadTree();
                }
            }
            if (menuItem.Name == "RemoveCellColumn")
            {
                var currenNod = treeView1.SelectedNode;
                iColumn headerColumn = currenNod.Tag as iColumn;
                iTable header = currenNod.Parent.Tag as iTable;
                if (header != null)
                {
                    if (headerColumn != null)
                    {
                        if (inv.RemoveColumn(header, headerColumn) == true)
                        {
                            currenNod.Remove();
                            var newnod = treeView1.SelectedNode;
                            treeView1_NodeMouseClick(treeView1, new TreeNodeMouseClickEventArgs(newnod, MouseButtons.Left, 1, 0, 0));
                            //LoadTree();
                        }

                    }
                }


            }
            //if (menuItem == "AddCsvData")
            //{

            //}

        }
        private void CreateContextMenu()

        {

            myContextMenuStrip = new ContextMenuStrip();

            ToolStripMenuItem menuItem = new ToolStripMenuItem("Add Table");

            menuItem.Click += new EventHandler(menuItem_Click);

            menuItem.Name = "AddTable";

            myContextMenuStrip.Items.Add(menuItem);
            menuItem = new ToolStripMenuItem("Add Cell");

            menuItem.Click += new EventHandler(menuItem_Click);

            menuItem.Name = "AddCellColumn";
            myContextMenuStrip.Items.Add(menuItem);
            menuItem = new ToolStripMenuItem("Remove Table");

            menuItem.Click += new EventHandler(menuItem_Click);

            menuItem.Name = "RemoveTable";
            myContextMenuStrip.Items.Add(menuItem);
            menuItem = new ToolStripMenuItem("Remove Column");

            menuItem.Click += new EventHandler(menuItem_Click);

            menuItem.Name = "RemoveCellColumn";
            myContextMenuStrip.Items.Add(menuItem);
            menuItem = new ToolStripMenuItem("Refresh");

            menuItem.Click += new EventHandler(menuItem_Click);

            menuItem.Name = "Refresh";
            myContextMenuStrip.Items.Add(menuItem);
            //this.ContextMenuStrip = myContextMenuStrip;


        }
        private void ShowContextMenuAddHeader(string[] menus)

        {
            foreach (ToolStripMenuItem itm in myContextMenuStrip.Items)
            {
                if (!menus.Contains(itm.Name))
                {
                    itm.Visible = false;
                }
                else
                {
                    itm.Visible = true;
                }
            }


        }
        private void CreateContextDataMenu()

        {

            myContextMenuStrip = new ContextMenuStrip();

            ToolStripMenuItem menuItem = new ToolStripMenuItem("Add Data Source");

            menuItem.Click += new EventHandler(dataMenuItem_Click);

            menuItem.Name = "AddDataSource";
            myContextMenuStrip.Items.Add(menuItem);
            menuItem = new ToolStripMenuItem("Add Parameters");

            menuItem.Click += new EventHandler(dataMenuItem_Click);

            menuItem.Name = "AddParameters";
            myContextMenuStrip.Items.Add(menuItem);

            menuItem = new ToolStripMenuItem("Refresh Tree");

            menuItem.Click += new EventHandler(dataMenuItem_Click);

            menuItem.Name = "Refresh";
            myContextMenuStrip.Items.Add(menuItem);
        }

        private void dataMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripItem menuItem = (ToolStripItem)sender;

            if (menuItem.Name == "AddDataSource")

            {
                if (inv.Document.DetailDataType == DataType.csv)
                {
                    Open_DetailCsvClick(true);
                }
                else
                {
                    Open_SqlConnectionDialog();
                }
                return;
            }
            if (menuItem.Name == "AddParameters")
            {
                var jsonParam = OpenParameter_Dialog(inv.Document.QueryParameter);
                if (jsonParam == "") return;
                var jobjParam = JsonConvert.DeserializeObject<JObject>(jsonParam.ToUpper());
                var dictParam = jobjParam.ToObject<Dictionary<string, object>>();
                inoicePrinting.InputParameters = dictParam;
                PrepareSqlReportData(inoicePrinting, inv, dictParam);
                return;
            }
            if (menuItem.Name == "Refresh")
            {
                LoadTree();
            }
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            //iHeaderColumn col = s as iHeaderColumn;
            if (treeView1.SelectedNode == null) return;
            iColumn tag = treeView1.SelectedNode.Tag as iColumn;
            if (tag != null)
            {
                treeView1.SelectedNode.Text = tag.Text;
                LoadPdf();
            }
        }

        private void Save_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "PdfInvoice|*.ims";
            saveFileDialog1.Title = "Save Pdf Invoice File";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {

                    //var jsonResolver = new PropertyRenameAndIgnoreSerializerContractResolver();
                    ////jsonResolver.IgnoreProperty(typeof(Invoice), "Title");
                    //jsonResolver.RenameProperty(typeof(Invoice), "DetailSource", "DetailCsvSource");

                    //var serializerSettings = new JsonSerializerSettings();

                    //serializerSettings.ContractResolver = jsonResolver;

                    //var json = JsonConvert.SerializeObject(person, serializerSettings);
                    var json = JsonConvert.SerializeObject(inv);
                    var filename = saveFileDialog1.FileName;
                    File.WriteAllText(filename, json);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void Open_Click(Object sender, EventArgs e)
        {
            openFileDialog1.Filter = "PdfInvoice|*.ims";
            openFileDialog1.Title = "Open Pdf Invoice File";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var filename = openFileDialog1.FileName;
                    string str = File.ReadAllText(filename);



                    inoicePrinting = new InvoicePrinting();
                    inv = inoicePrinting.LoadInvFromJson(str);
                    LoadTree();
                    LoadPdf();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void New_Click(object sender, EventArgs e)
        {
            inv = new Invoice();
            LoadTree();
            pdfDocumentViewer1.CloseDocument();

        }

        private void Open_DetailCsvClick(bool isDetail)
        {
            openFileDialog1.Filter = "Csv Fils|*.csv";
            openFileDialog1.Title = "Open csv Data File";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var filename = openFileDialog1.FileName;

                    if (isDetail == true)
                    {
                        inv.AddDetailcsvData(filename);
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void Open_SqlConnectionDialog()
        {
            SqlConnectionDialog dlg = new SqlConnectionDialog();
            dlg.ConnectionString = inv.Document.ConnectionString;
            dlg.DetailQuery = inv.Document.DetailSource;
            dlg.ReportQuery = inv.Document.ReportSource;
            dlg.Parameter = inv.Document.QueryParameter;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var constring = dlg.ConnectionString;
                var detailQuery = dlg.DetailQuery;
                var reportQuery = dlg.ReportQuery;
                var parameter = dlg.Parameter;
                inv.Document.setDetailSource(detailQuery);
                inv.Document.setSqlConnection(constring);
                inv.Document.setReportSource(reportQuery);
                inv.Document.setQueryParameter(parameter);

            }



        }

        private void Export_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog1.Filter = "pdf|*.pdf";
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    inoicePrinting.PrintInvoice(inv, saveFileDialog1.FileName);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }

    public class PropertyRenameAndIgnoreSerializerContractResolver : DefaultContractResolver
    {
        private readonly Dictionary<Type, HashSet<string>> _ignores;
        private readonly Dictionary<Type, Dictionary<string, string>> _renames;

        public PropertyRenameAndIgnoreSerializerContractResolver()
        {
            _ignores = new Dictionary<Type, HashSet<string>>();
            _renames = new Dictionary<Type, Dictionary<string, string>>();
        }

        public void IgnoreProperty(Type type, params string[] jsonPropertyNames)
        {
            if (!_ignores.ContainsKey(type))
                _ignores[type] = new HashSet<string>();

            foreach (var prop in jsonPropertyNames)
                _ignores[type].Add(prop);
        }

        public void RenameProperty(Type type, string propertyName, string newJsonPropertyName)
        {
            if (!_renames.ContainsKey(type))
                _renames[type] = new Dictionary<string, string>();

            _renames[type][propertyName] = newJsonPropertyName;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (IsIgnored(property.DeclaringType, property.PropertyName))
            {
                property.ShouldSerialize = i => false;
                property.Ignored = true;
            }

            if (IsRenamed(property.DeclaringType, property.PropertyName, out var newJsonPropertyName))
                property.PropertyName = newJsonPropertyName;

            return property;
        }

        private bool IsIgnored(Type type, string jsonPropertyName)
        {
            if (!_ignores.ContainsKey(type))
                return false;

            return _ignores[type].Contains(jsonPropertyName);
        }

        private bool IsRenamed(Type type, string jsonPropertyName, out string newJsonPropertyName)
        {
            Dictionary<string, string> renames;

            if (!_renames.TryGetValue(type, out renames) || !renames.TryGetValue(jsonPropertyName, out newJsonPropertyName))
            {
                newJsonPropertyName = null;
                return false;
            }

            return true;
        }
    }
}
