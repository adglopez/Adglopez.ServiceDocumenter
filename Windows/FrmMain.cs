using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Service_Documenter
{
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var metadataReader = new Adglopez.ServiceDocumenter.Core.Metadata.MetadataReader();
            var exporter = new Adglopez.ServiceDocumenter.Exporters.Excel.Expoter();
            var serviceInfo = metadataReader.ParseMetadata(txtUrl.Text);

            exporter.Export(serviceInfo, txtDocument.Text);
            Process.Start("Excel.exe","\"" + txtDocument.Text + "\"");
        }
    }
}
