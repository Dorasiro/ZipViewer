using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZipViewer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();


            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void LoadZipFile(string filePath)
        {
            if(Path.GetExtension(filePath) != ".zip")
            {
                throw new ArgumentException("zipファイルじゃない");
            }

            mainFlowLayoutPanel.Controls.Clear();

            var z = ZipFile.Open(filePath, ZipArchiveMode.Read, System.Text.Encoding.GetEncoding("Shift_JIS"));
            foreach (ZipArchiveEntry entry in z.Entries)
            {
                PictureBox p = new PictureBox();
                p.Size = new Size(100, 120);
                p.SizeMode = PictureBoxSizeMode.CenterImage;
                using (var img = Image.FromStream(entry.Open()))
                {
                    p.Image = ThumbMaker.MakeThumb(img, 100, 120);
                    mainFlowLayoutPanel.Controls.Add(p);
                }
            }
        }

        private void flowLayoutPanel1_DragEnter(object sender, DragEventArgs e)
        {
            // DragEnterにこれを書くことでファイルのみ受け入れ可能になるらしい
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // 実際に受け入れをするのはここ
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void flowLayoutPanel1_DragDrop(object sender, DragEventArgs e)
        {
            LoadZipFile(((string[])e.Data.GetData(DataFormats.FileDrop))[0]);
        }
    }
}
