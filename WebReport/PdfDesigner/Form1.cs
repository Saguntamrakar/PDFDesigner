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
using RawPrint;
using PDfCreator.Helper;

namespace PdfDesigner
{
    public partial class Form1 : Form
    {
        Invoice inv;
        ContextMenuStrip myContextMenuStrip;
        InvoicePrinting inoicePrinting;
        private CutObject CutItem = null;
        private class CutObject
        {
            public int CutObjIndex { get; set; }
            public iTable CutParentobj { get; set; }
            public TreeNode CutNode { get; set; }
        }
        public Form1()
        {
            InitializeComponent();
            CreateContextMenu();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            inv = new Invoice();
            inoicePrinting = new InvoicePrinting();
            foreach (string printername in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
            {
                this.Printers.Items.Add(printername);
            }
        }
        private void LoadTree()
        {
            treeView1.Nodes.Clear();
            var doc = treeView1.Nodes.Add("Document");
            doc.Tag = inv.Document;
            doc.ToolTipText = "Right Click for menu";
            //Report Header Section
            var headernod = treeView1.Nodes.Add("Headers");
            headernod.Tag = inv.ReportHeaders;
            int i = 0;
            foreach (var head in inv.ReportHeaders)
            {
                i++;
                var header = headernod.Nodes.Add($"Header({i})");
                header.Tag = head;
                header.ToolTipText = "Right Click for Menu";
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
            detail.ToolTipText = "Right Click for Menu";
            TreeNode detailHeader = detail.Nodes.Add("Detail Header");
            detailHeader.Tag = inv.Detail.DetailHeader;
            detailHeader.ToolTipText = "Right Click for Menu";
            foreach (var col in inv.Detail.DetailHeader.Columns)
            {
                string colName = col.GetType().Equals(typeof(iColumn)) ? ((iColumn)col).Text : "Table";
                var column = detailHeader.Nodes.Add(colName);
                column.ToolTipText = "Right Click for Menu";
                column.Tag = col;
            }
            TreeNode detailSection = detail.Nodes.Add("Data");
            detailSection.Tag = inv.Detail.Detail;
            detailSection.ToolTipText = "Right Click for Menu";
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
                    colnod.ToolTipText = "Right Click for Menu";
                    c++;
                }
            }

            TreeNode footerSection = detail.Nodes.Add("Detail Footer");
            footerSection.Tag = inv.Detail.DetailFooter;
            footerSection.ToolTipText = "Right Click for Menu";
            foreach (var col in inv.Detail.DetailFooter.Columns)
            {
                string colName = col.GetType().Equals(typeof(iColumn)) ? ((iColumn)col).Text : "Table";
                var column = footerSection.Nodes.Add(colName);
                column.ToolTipText = "Right Click for Menu";
                column.Tag = col;
            }

            //Report Footer Section
            var footerernod = treeView1.Nodes.Add("Footers");
            footerernod.Tag = inv.ReportFooters;
            footerernod.ToolTipText = "Right Click for Menu";
            i = 0;
            foreach (var foot in inv.ReportFooters)
            {
                i++;
                var footer = footerernod.Nodes.Add($"Footer({i})");
                footer.Tag = foot;
                footer.ToolTipText = "Right Click for Menu";
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
                nod.ToolTipText = "Right Click for Menu";
            }
            else if (col.GetType().Equals(typeof(iTable)))
            {
                var nod = parentNode.Nodes.Add("Table");
                nod.Tag = col;
                nod.ToolTipText = "Right Click for Menu";
                foreach (var columns in ((iTable)col).Columns)
                {
                    LoadNode(columns, nod);
                }
            }
            else if (col.GetType().Equals(typeof(iImage)))
            {
                var nod = parentNode.Nodes.Add("Image");
                nod.Tag = (iImage)col;
                nod.ToolTipText = "Right Click for Menu";
            }

        }
        private Stream LoadPdf(string ExportFileName = "")
        {
            Stream stream = new MemoryStream();
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
                if (inoicePrinting.InputParameters == null)
                {
                    if (string.IsNullOrEmpty(inv.Document.QueryParameter) == false)
                    {
                        string paramString = inv.Document.QueryParameter;
                        jsonParam = OpenParameter_Dialog(paramString);
                        var jobjParam = JsonConvert.DeserializeObject<JObject>(jsonParam);
                        dictParam = jobjParam.ToObject<Dictionary<string, object>>();
                        inoicePrinting.InputParameters = dictParam;
                    }
                }
                else
                {
                    dictParam = inoicePrinting.InputParameters;
                }
                PrepareSqlReportData(inoicePrinting, inv, dictParam);
                //if (string.IsNullOrEmpty(inv.Document.DetailSource))
                //{
                //    if (inoicePrinting.DetailData == null || inoicePrinting.DetailData.Count() == 0)
                //    {


                //    }
                //}




                inoicePrinting.StartRow = 0;
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    byte[] bytes = inoicePrinting.PrintInvoice(inv, DEST, memoryStream);
                    //byte[] bytes = memoryStream.ToArray();
                    stream = new MemoryStream(bytes);
                    pdfDocumentViewer1.LoadFromStream(stream);
                    //pdfDocumentViewer1.LoadFromFile(DEST);
                }

                return stream;

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
                return stream;
            }
        }
        private async Task<Stream> LoadPdfAsync(string ExportFileName = "")
        {
            Stream stream = new MemoryStream();
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
                if (inoicePrinting.InputParameters == null)
                {
                    if (string.IsNullOrEmpty(inv.Document.QueryParameter) == false)
                    {
                        string paramString = inv.Document.QueryParameter;
                        jsonParam = OpenParameter_Dialog(paramString);
                        var jobjParam = JsonConvert.DeserializeObject<JObject>(jsonParam);
                        dictParam = jobjParam.ToObject<Dictionary<string, object>>();
                        inoicePrinting.InputParameters = dictParam;
                    }
                }
                else
                {
                    dictParam = inoicePrinting.InputParameters;
                }
                PrepareSqlReportData(inoicePrinting, inv, dictParam);
                //if (string.IsNullOrEmpty(inv.Document.DetailSource))
                //{
                //    if (inoicePrinting.DetailData == null || inoicePrinting.DetailData.Count() == 0)
                //    {


                //    }
                //}






                byte[] bytes = await inoicePrinting.PrintInvoiceAsync(inv);
                //byte[] bytes = memoryStream.ToArray();
                stream = new MemoryStream(bytes);
                pdfDocumentViewer1.LoadFromStream(stream);
                //pdfDocumentViewer1.LoadFromFile(DEST);


                return stream;

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
                return stream;
            }
        }

        private void PrepareSqlReportData(InvoicePrinting invPrint, Invoice inv, IDictionary<string, object> param)
        {
            var constring = inv.Document.getSqlConnectionString();
            var reportQuery = inv.Document.ReportSource;
            var detailQuery = inv.Document.DetailSource;

            using (SqlConnection con = new SqlConnection(constring))
            {

                IDictionary<string, object> reportData = null;
                if (string.IsNullOrEmpty(reportQuery) == false)
                {
                    if (invPrint.ReportData == null)
                    {
                        reportData = (IDictionary<string, object>)con.Query(reportQuery, param).FirstOrDefault();
                        invPrint.ReportData = reportData;
                    }
                }
                if (string.IsNullOrEmpty(detailQuery) == false)
                {
                    if (invPrint.DetailData == null)
                    {
                        List<IDictionary<string, object>> detailData = con.Query(detailQuery, param).Select(row => (IDictionary<string, object>)row).ToList();
                        invPrint.DetailData = detailData;
                    }
                }


            }
        }

        private string OpenParameter_Dialog(string paramString)
        {
            if (string.IsNullOrEmpty(paramString)) return "";
            try
            {
                ParameterDialog pmDlg = new ParameterDialog();
                pmDlg.DialogParamList = inv.Document.DialogParamList;
                pmDlg.LoadParameters(paramString, inoicePrinting.InputParameters);
                if (pmDlg.ShowDialog() == DialogResult.OK)
                {
                    string jsonParam = pmDlg.jsonParam;
                    inv.Document.DialogParamList = pmDlg.DialogParamList;
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

        private void OpenDialogParameter_dialog(string paramString)
        {
            if (string.IsNullOrEmpty(paramString)) return ;
            try
            {
                DialogDesigner pmDlg = new DialogDesigner();
                //var dlglst = inv.Document.DialogParamList.ToList();
               
                pmDlg.DialogParamList = inv.Document.DialogParamList;
                pmDlg.LoadParameterDialogs(paramString, inoicePrinting.InputParameters);
                if (pmDlg.ShowDialog() == DialogResult.OK)
                {
                    inv.Document.DialogParamList = pmDlg.DialogParamList;
                    
                }
                
            }
            catch (Exception Ex)
            {

                MessageBox.Show(Ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            LoadPdfAsync();
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
            else if (obj.GetType().Equals(typeof(iImage)))
            {
                propertyGrid1.SelectedObject = (iImage)obj;
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
            if (nod.Tag.GetType().Equals(typeof(iDocument)) || nod.Tag.GetType().Equals(typeof(iTable)) || nod.Tag.GetType().Equals(typeof(iDetail)) || nod.Tag.GetType().Equals(typeof(iFixedColTable)) || nod.Tag.GetType().Equals(typeof(iImage)))
            {
                LoadPropertyGrid(nod.Tag);
                return;
            }

            LoadPropertyGrid(nod.Text);

        }

        private void button1_Click_2(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(inv.Document.QueryParameter) == false)
            {
                string paramString = inv.Document.QueryParameter;
                var jsonParam = OpenParameter_Dialog(paramString);
                if (jsonParam.ToString() == "") return;
                var jobjParam = JsonConvert.DeserializeObject<JObject>(jsonParam);
                var dictParam = jobjParam.ToObject<Dictionary<string, object>>();
                inoicePrinting.InputParameters = dictParam;
                inoicePrinting.ReportData = null;
                inoicePrinting.DetailData = null;
                inoicePrinting.StartRow = 0;
            }
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
                    ShowContextMenuAddHeader(new string[] { "AddTable", "AddCellColumn", "AddCellImange" }, null);
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
                        if (treeView1.SelectedNode.Text == "Data" || treeView1.SelectedNode.Text == "Detail Header" || treeView1.SelectedNode.Text == "Detail Footer")
                        {
                            ShowContextMenuAddHeader(new string[] { "AddCellColumn" }, tag);
                        }
                        else
                        {
                            ShowContextMenuAddHeader(new string[] { "AddCellColumn", "AddCellImage", "AddTable", "RemoveTable", "Cut", "Paste" }, tag);
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
                        ShowContextMenuAddHeader(new string[] { "Cut", "Paste", "RemoveCellColumn" }, tag);
                        myContextMenuStrip.Show(treeView1, e.Location);
                    }

                    if (tag.GetType().Equals(typeof(iImage)))
                    {
                        ShowContextMenuAddHeader(new string[] { "RemoveCellImage" }, tag);
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
            try
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
                if (menuItem.Name == "AddCellImage")
                {
                    iTable header = treeView1.SelectedNode.Tag as iTable;
                    if (header != null)
                    {
                        string filename = GetImageFile();
                        if (filename == "") return;
                        var col = inv.NewImage(header, filename);
                        var newnod = selectedNod.Nodes.Add("Image");
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

                if (menuItem.Name == "RemoveCellImage")
                {
                    var currenNod = treeView1.SelectedNode;
                    iImage headerColumn = currenNod.Tag as iImage;
                    iTable header = currenNod.Parent.Tag as iTable;
                    if (header != null)
                    {
                        if (headerColumn != null)
                        {
                            if (inv.RemoveImage(header, headerColumn) == true)
                            {
                                currenNod.Remove();
                                var newnod = treeView1.SelectedNode;
                                treeView1_NodeMouseClick(treeView1, new TreeNodeMouseClickEventArgs(newnod, MouseButtons.Left, 1, 0, 0));
                                //LoadTree();
                            }

                        }
                    }


                }

                if (menuItem.Name == "Cut")
                {
                    var currenNod = treeView1.SelectedNode;
                    if (currenNod.Tag == null) { CutItem = null; return; };

                    var parrentNod = currenNod.Parent;
                    iTable parentTable = parrentNod.Tag as iTable;
                    if (parentTable == null) return;
                    var ind = parentTable.Columns.IndexOf(currenNod.Tag);

                    CutItem = new CutObject() { CutNode = currenNod, CutObjIndex = ind, CutParentobj = parentTable };


                }

                if (menuItem.Name == "Paste")
                {
                    var currenNod = treeView1.SelectedNode;
                    if (currenNod.Tag == null) throw new Exception("selected item is null");
                    if (CutItem == null) throw new Exception("Cut item not found");
                    var parentNod = treeView1.SelectedNode.Parent;
                    var parentItem = parentNod.Tag;
                    if (parentItem == null) throw new Exception("Parent is not found");
                    //if (parentNod.Text == "Data") throw new Exception("Data section cannot be pasted");
                    //if (!CutItem.CutObj.GetType().Equals(currenNod.Tag.GetType())) throw new Exception("Cut item and paste item do not match");
                    var CutObj = CutItem.CutParentobj.Columns[CutItem.CutObjIndex];
                    if (CutObj.GetType().Equals(typeof(iColumn)) || CutObj.GetType().Equals(typeof(iTable)))
                    {
                        if (CutObj.GetType().Equals(typeof(iTable)) && (parentNod.Text == "Detail Header" || parentNod.Text == "Detail Footer" || parentNod.Text == "Data"))
                        {
                            throw new Exception("Table cannot be paste here");
                        }
                        //{
                        var parenttbl = parentItem as iTable;
                        if (parenttbl == null) throw new Exception("Parent is not a table");

                        CutItem.CutParentobj.Columns.RemoveAt(CutItem.CutObjIndex);
                        var arrayIndex = parenttbl.Columns.IndexOf(currenNod.Tag);
                        parenttbl.Columns.Insert(arrayIndex, CutObj);
                        var nodIndex = selectedNod.Index;
                        string text = CutObj.GetType().Equals(typeof(iColumn)) ? ((iColumn)CutObj).Text : "Table";
                        var insertedNod = parentNod.Nodes.Insert(nodIndex, text);
                        insertedNod.Tag = CutObj;
                        CutItem.CutNode.Parent.Nodes.Remove(CutItem.CutNode);
                        CutItem = null;

                        //}
                    }

                    //if (currenNod.Tag.GetType().Equals(typeof(iTable)))
                    //{
                    //    CutObject = currenNod.Tag as iTable;
                    //}




                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private string GetImageFile()
        {
            try
            {
                openFileDialog1.Filter = "JPG|*.jpg|PNG|*.png|BMP|*.bmp";
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    return openFileDialog1.FileName;
                }
                return "";
            }
            catch
            {
                return "";
            }
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
            menuItem = new ToolStripMenuItem("Add Image");
            menuItem.Click += new EventHandler(menuItem_Click);
            menuItem.Name = "AddCellImage";
            myContextMenuStrip.Items.Add(menuItem);

            menuItem = new ToolStripMenuItem("Remove Image");
            menuItem.Click += new EventHandler(menuItem_Click);
            menuItem.Name = "RemoveCellImage";
            myContextMenuStrip.Items.Add(menuItem);

            menuItem = new ToolStripMenuItem("Cut");
            menuItem.Click += new EventHandler(menuItem_Click);
            menuItem.Name = "Cut";
            myContextMenuStrip.Items.Add(menuItem);

            menuItem = new ToolStripMenuItem("Paste");
            menuItem.Click += new EventHandler(menuItem_Click);
            menuItem.Name = "Paste";
            myContextMenuStrip.Items.Add(menuItem);

        }
        private void ShowContextMenuAddHeader(string[] menus, object sender)

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
                    if (itm.Name == "Paste")
                    {
                        itm.Visible = false;
                        if (CutItem != null)
                        {
                            itm.Visible = true;
                        }
                    }

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

            menuItem = new ToolStripMenuItem("Add Parameter Dialog Designer");
            menuItem.Click += new EventHandler(dataMenuItem_Click);
            menuItem.Name = "ParamDialog";
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
                var jobjParam = JsonConvert.DeserializeObject<JObject>(jsonParam);
                var dictParam = jobjParam.ToObject<Dictionary<string, object>>();
                inoicePrinting.InputParameters = dictParam;
                PrepareSqlReportData(inoicePrinting, inv, dictParam);
                return;
            }
            if (menuItem.Name == "ParamDialog")
            {
                OpenDialogParameter_dialog(inv.Document.QueryParameter);

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
                    inv = inoicePrinting.LoadInvFromJsonString(str);
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
            dlg.Server = inv.Document.Server;
            dlg.Database = inv.Document.Database;
            dlg.User = inv.Document.User;
            dlg.Password = Encryption.Decrypt(inv.Document.Password == null ? "" : inv.Document.Password);
            dlg.DetailQuery = inv.Document.DetailSource;
            dlg.ReportQuery = inv.Document.ReportSource;
            dlg.Parameter = inv.Document.QueryParameter;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var server = dlg.Server;
                var database = dlg.Database;
                var user = dlg.User;
                var password = dlg.Password;
                var detailQuery = dlg.DetailQuery;
                var reportQuery = dlg.ReportQuery;
                var parameter = dlg.Parameter;
                inv.Document.setDetailSource(detailQuery);
                inv.Document.setServer(server);
                inv.Document.setDatabase(database);
                inv.Document.setUser(user);
                inv.Document.setPassword(Encryption.Encrypt(password));
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
                    using (MemoryStream memstream = new MemoryStream())
                    {
                        var bytes = inoicePrinting.PrintInvoice(inv, saveFileDialog1.FileName, memstream);
                        File.WriteAllBytes(saveFileDialog1.FileName, bytes);
                    }
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string printername = this.Printers.SelectedItem as string;
            if (string.IsNullOrEmpty(printername)) printername = System.Drawing.Printing.PrinterSettings.InstalledPrinters[0];
            Stream stream = LoadPdf();
            Printer.PrintStream(printername, stream, "pdfdocument");
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
