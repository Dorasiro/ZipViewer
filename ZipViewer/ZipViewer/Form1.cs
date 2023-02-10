using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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

            var z = ZipFile.Open(@"D:\Dorashiro\デスクトップ\新しいフォルダー (2).zip", ZipArchiveMode.Read, System.Text.Encoding.GetEncoding("Shift_JIS"));
            foreach (ZipArchiveEntry e in z.Entries)
            {
                PictureBox p = new PictureBox();
                p.Size = new Size(100, 120);
                using (var img = Image.FromStream(e.Open()))
                {
                    p.Image = ThumbMaker.MakeThumb(img, 100, 120);
                    flowLayoutPanel1.Controls.Add(p);
                }
            }
        }
    }
}
