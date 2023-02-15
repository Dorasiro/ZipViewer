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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ZipViewer
{
    public partial class MainForm : Form
    {
        public static readonly string[] CanReadImageFormatArray = {".png", ".jpg", ".jpeg", ".bmp"};

        public MainForm()
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

            // Zipファイルの名前を表示するラベルを作る
            var zipFileName = new Label()
            {
                Text = Path.GetFileName(filePath),
                AutoSize = true,
            };

            // マウスを乗せたらフルパスを表示する
            var tip = new System.Windows.Forms.ToolTip();
            tip.InitialDelay = 200;
            tip.SetToolTip(zipFileName, filePath);
            
            mainFlowLayoutPanel.Controls.Add(zipFileName);

            // 画像表示用のパネルを作る
            var showImgFlowLayoutPanel = new FlowLayoutPanel()
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
                AutoSize = true,
            };

            // zipを開く
            var z = ZipFile.Open(filePath, ZipArchiveMode.Read, System.Text.Encoding.GetEncoding("Shift_JIS"));
            foreach (ZipArchiveEntry entry in z.Entries)
            {
                // サムネ画像の表示スペース
                PictureBox p = new PictureBox()
                {
                    Size = new Size(100, 120),
                    SizeMode = PictureBoxSizeMode.CenterImage
                };
                
                // 読み込める拡張子か判定
                if(CanReadImageFormatArray.Contains(Path.GetExtension(entry.Name)))
                {
                    using (var img = Image.FromStream(entry.Open()))
                    {
                        p.Image = ThumbMaker.MakeThumb(img, 100, 120);
                    }
                }
                // 読み込めない拡張子のとき
                else
                {
                    // アイコン用のディレクトリを作る
                    if(!Directory.Exists("icon"))
                    {
                        Directory.CreateDirectory("icon");
                    }

                    var iconFilePath = @"icon\icon" + Path.GetExtension(entry.Name);

                    // icon用の空ファイルがなければ作る
                    if (!File.Exists(iconFilePath))
                    {
                        File.Create(iconFilePath);
                    }

                    // zipの中のファイルのアイコンを取得できないっぽいから拡張子だけ合わせた空ファイルを作ってそれのアイコンを拾ってくる仕組み
                    using (Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(iconFilePath))
                    {
                        p.Image = icon.ToBitmap();
                    }
                }

                // 画像ファイル名を表示するラベル
                var imgNameLabel = new Label()
                {
                    Width = 100,
                    Text = entry.Name,
                };

                // 画像とファイル名を1セットにしたパネル
                var imgAndNameTableLayoutPanel = new TableLayoutPanel()
                {
                    AutoSize = true,
                    ColumnCount = 1,
                    RowCount = 2,
                    Anchor = AnchorStyles.Left,
                };

                imgAndNameTableLayoutPanel.Controls.Add(p);
                imgAndNameTableLayoutPanel.Controls.Add(imgNameLabel);

                showImgFlowLayoutPanel.Controls.Add(imgAndNameTableLayoutPanel);
            }

            mainFlowLayoutPanel.Controls.Add(showImgFlowLayoutPanel);
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
