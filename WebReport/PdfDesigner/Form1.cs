using Newtonsoft.Json;
using PDfConsole;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PdfDesigner
{
    public partial class Form1 : Form
    {
        Invoice inv;
        ContextMenuStrip myContextMenuStrip;
        public Form1()
        {
            InitializeComponent();
            CreateContextMenu();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            inv = new Invoice();
            //inv.NewHeader();
            //inv.NewColumn(inv.Headers[0]);
            //inv.Headers[0].Columns[0].width = 2;
            //inv.Headers[0].Columns[0].Text = "What is your name";
            //inv.Headers[0].Columns[0].IsBold = true;
            //inv.Headers[0].Columns[0].FontSize = 14;
            //inv.Headers[0].Columns[0].NoLeftBorder = true;
            //inv.NewColumn(inv.Headers[0]);
            //inv.Headers[0].Columns[1].width = 5;
            //inv.Headers[0].Columns[1].Text = "This is my name";
            //inv.Headers[0].Columns[1].IsBold = true;
            //inv.Headers[0].Columns[1].NoBorder = true;
            //inv.Headers[0].Columns[1].FontSize = 7;
            //inv.NewColumn(inv.Headers[0]);
            //inv.Headers[0].Columns[2].width = 5;
            //inv.Headers[0].Columns[2].Text = "This is my name";
            //inv.Headers[0].Columns[2].IsBold = true;
            //inv.Headers[0].Columns[2].FontSize = 14;
            //inv.NewHeader();
            //inv.NewColumn(inv.Headers[1]);
            //inv.Headers[1].Columns[0].width = 2;
            //inv.Headers[1].Columns[0].Text = "What is your name";
            //inv.Headers[1].Columns[0].IsBold = true;
            //inv.Headers[1].Columns[0].FontSize = 14;
            //inv.NewColumn(inv.Headers[1]);
            //inv.Headers[1].Columns[1].width = 5;
            //inv.Headers[1].Columns[1].Text = "This is my name";
            //inv.Headers[1].Columns[1].IsBold = true;
            //inv.Headers[1].Columns[1].NoBorder = true;
            //inv.Headers[1].Columns[1].FontSize = 14;
            //inv.NewColumn(inv.Headers[1]);
            //inv.Headers[1].Columns[2].width = 5;
            //inv.Headers[1].Columns[2].Text = "This is my name";
            //inv.Headers[1].Columns[2].IsBold = true;
            //inv.Headers[1].Columns[2].FontSize = 14;
            //LoadTree();
            //LoadPdf();
        }
        private void LoadTree()
        {
            treeView1.Nodes.Clear();
            var headernod = treeView1.Nodes.Add("Headers");
            int i = 0;
            foreach (var head in inv.Headers)
            {
                i++;
                var header = headernod.Nodes.Add($"Header({i})");
                header.Tag = head;
                foreach (var col in head.Columns)
                {
                    var column = header.Nodes.Add(col.Text);
                    column.Tag = col;
                }
            }
        }
        private void LoadPdf()
        {
            try
            {
                string DEST = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test1.pdf");
                FileInfo file = new FileInfo(DEST);

                InoicePrinting inoicePrinting = new InoicePrinting(DEST, "A4");
                inoicePrinting.PrintInvoice(inv, DEST);

                pdfDocumentViewer1.LoadFromFile(DEST);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
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
            propertyGrid1.SelectedObject = obj;
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode nod = e.Node;
            if (nod.Tag == null) { LoadPropertyGrid(null); return; }
            if (nod.Tag.GetType().Equals(typeof(iHeaderColumn)))
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
                if (treeView1.SelectedNode.Text == "Headers")
                {
                    ShowContextMenuAddHeader(new string[] { "AddHeader" });
                    treeView1.ContextMenuStrip = myContextMenuStrip;
                    myContextMenuStrip.Show(treeView1, e.Location);
                    return;
                }
                var tag = treeView1.SelectedNode.Tag;
                if (tag != null)
                {
                    if (tag.GetType().Equals(typeof(iHeader)))
                    {
                        ShowContextMenuAddHeader(new string[] { "AddHeaderColumn", "RemoveHeader" });
                        myContextMenuStrip.Show(treeView1, e.Location);
                    }
                    if (tag.GetType().Equals(typeof(iHeaderColumn)))
                    {
                        ShowContextMenuAddHeader(new string[] { "RemoveHeaderColumn" });
                        myContextMenuStrip.Show(treeView1, e.Location);
                    }
                    return;
                }

            }
        }

        void menuItem_Click(object sender, EventArgs e)

        {

            ToolStripItem menuItem = (ToolStripItem)sender;

            if (menuItem.Name == "AddHeader")

            {

                inv.NewHeader();
                LoadTree();

            }
            if (menuItem.Name == "AddHeaderColumn")
            {
                iHeader header = treeView1.SelectedNode.Tag as iHeader;
                if (header != null)
                {
                    inv.NewColumn(header);
                    LoadTree();
                }


            }
            if (menuItem.Name == "RemoveHeader")
            {
                iHeader header = treeView1.SelectedNode.Tag as iHeader;
                if (header != null)
                {
                    inv.RemoveHeader(header);
                    LoadTree();
                }
            }
            if (menuItem.Name == "RemoveHeaderColumn")
            {
                iHeaderColumn headerColumn = treeView1.SelectedNode.Tag as iHeaderColumn;
                iHeader header = treeView1.SelectedNode.Parent.Tag as iHeader;
                if (header != null)
                {
                    if (headerColumn != null)
                    {
                        inv.RemoveColumn(header, headerColumn);
                        LoadTree();
                    }
                }


            }

        }
        private void CreateContextMenu()

        {

            myContextMenuStrip = new ContextMenuStrip();

            ToolStripMenuItem menuItem = new ToolStripMenuItem("Add Header");

            menuItem.Click += new EventHandler(menuItem_Click);

            menuItem.Name = "AddHeader";

            myContextMenuStrip.Items.Add(menuItem);
            menuItem = new ToolStripMenuItem("Add Column");

            menuItem.Click += new EventHandler(menuItem_Click);

            menuItem.Name = "AddHeaderColumn";
            myContextMenuStrip.Items.Add(menuItem);
            menuItem = new ToolStripMenuItem("Remove Header");

            menuItem.Click += new EventHandler(menuItem_Click);

            menuItem.Name = "RemoveHeader";
            myContextMenuStrip.Items.Add(menuItem);
            menuItem = new ToolStripMenuItem("Remove Column");

            menuItem.Click += new EventHandler(menuItem_Click);

            menuItem.Name = "RemoveHeaderColumn";
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
        private void CreateContextRemoveHeaderColumn()

        {

            myContextMenuStrip = new ContextMenuStrip();

            ToolStripMenuItem menuItem = new ToolStripMenuItem("Remove Column");

            menuItem.Click += new EventHandler(menuItem_Click);

            menuItem.Name = "RemoveHeaderColumn";
            myContextMenuStrip.Items.Add(menuItem);
            this.ContextMenuStrip = myContextMenuStrip;

        }
        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            //iHeaderColumn col = s as iHeaderColumn;
            if (treeView1.SelectedNode == null) return;
            iHeaderColumn tag = treeView1.SelectedNode.Tag as iHeaderColumn;
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
                    var json = JsonConvert.DeserializeObject<Invoice>(str);
                    if (json == null) throw new Exception("File format is not correct. It cannot be open");
                    inv = json;
                    LoadTree();
                    LoadPdf();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}
