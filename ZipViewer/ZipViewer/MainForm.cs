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
using ZipViewer.GUI;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ZipViewer
{
    public partial class MainForm : Form, IImgController
    {
        public static readonly string[] CanReadImageFormatArray = {".png", ".jpg", ".jpeg", ".bmp"};
        private List<string> readFileList;
        private List<FlowLayoutPanel> showImgFlowLayoutPanelList;
        private List<Panel> splitLinePanelList;
        // 画像の表示順序を決めるためのリスト
        private List<string> imgNameList;
        // ファイル名と画像を関連付ける辞書
        private Dictionary<string, Image> imgDict;
        // 今表示中の画像ファイルの名前
        private string nowShowingImg;

        // マウスを乗せた時の色
        public static readonly Color FileSelectColor = Color.LightBlue;
        // クリックして選択したときの色
        public static readonly Color FileClickColor = Color.LightSkyBlue;

        public MainForm()
        {
            InitializeComponent();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            readFileList= new List<string>();
            showImgFlowLayoutPanelList = new List<FlowLayoutPanel>();
            splitLinePanelList = new List<Panel>();
            imgNameList = new List<string>();
            imgDict= new Dictionary<string, Image>();
        }

        public void LoadZipFile(string filePath)
        {
            if(!File.Exists(filePath))
            {
                var hook = new FormMessageBoxHook();
                hook.SetHook(this);
                MessageBox.Show("ファイルが存在しない、もしくはフォルダなので無理です。");
                return;
            }

            if(Path.GetExtension(filePath) != ".zip")
            {
                var hook = new FormMessageBoxHook();
                hook.SetHook(this);
                MessageBox.Show("zipファイルじゃないのは無理です。");
                return;
            }

            if(readFileList.Contains(filePath))
            {
                var hook = new FormMessageBoxHook();
                hook.SetHook(this);
                MessageBox.Show("そのファイルは既に読み込まれています。"+Environment.NewLine+filePath);
                return;
            }

            readFileList.Add(filePath);

            // Zipファイルの名前を表示するラベルを作る
            var zipFileName = new Label()
            {
                Text = Path.GetFileName(filePath),
                AutoSize = true,
                Font = new Font("ＭＳ Ｐゴシック", 16, FontStyle.Bold),
            };

            // 読み込んだ時pファイルが２個目以上の時はマージンの幅を変える
            if(showImgFlowLayoutPanelList.Count > 0)
            {
                zipFileName.Margin = new Padding(0, 20, 0, 0);

                var p = new Panel()
                {
                    BorderStyle = BorderStyle.FixedSingle,
                    //Anchor = AnchorStyles.Left | AnchorStyles.Right,
                    Height = 1,
                    Width = mainFlowLayoutPanel.ClientSize.Width-7,
                };
                splitLinePanelList.Add(p);

                mainFlowLayoutPanel.Controls.Add(p);
            }

            // マウスを乗せたらフルパスを表示する
            var zipTip = new System.Windows.Forms.ToolTip();
            zipTip.InitialDelay = 200;
            zipTip.SetToolTip(zipFileName, filePath);
            
            mainFlowLayoutPanel.Controls.Add(zipFileName);

            // 画像表示用のパネルを作る
            var showImgFlowLayoutPanel = new FlowLayoutPanel()
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                AutoSize = true,
                MaximumSize = new Size(mainFlowLayoutPanel.Width - 7, 9999999)
            };
            showImgFlowLayoutPanelList.Add(showImgFlowLayoutPanel);

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
                    // imgをフィールドに保存するため、Disposeしていない。表示中のzipを消す処理をつけた時にDisposeすること。
                    var img = Image.FromStream(entry.Open());
                    p.Image = ThumbMaker.MakeThumb(img, 100, 120);

                    // ImgViewer用
                    var imgName = filePath + "\\" + entry.Name;
                    imgNameList.Add(imgName);
                    imgDict.Add(imgName, img);
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

                p.MouseEnter += (sender, e) =>
                {
                    p.Parent.BackColor = FileSelectColor;
                };

                p.MouseLeave += (sender, e) =>
                {
                    p.Parent.BackColor = DefaultBackColor;
                };

                p.MouseClick += (sender, e) =>
                {
                    var imgViewer = new ImgViewer(this);
                    nowShowingImg = filePath + "\\" + entry.Name;
                    imgViewer.SetImage(imgDict[nowShowingImg]);
                    imgViewer.ShowDialog(this);

                };

                // 画像ファイル名を表示するラベル
                var imgNameLabel = new Label()
                {
                    Width = 100,
                    Text = entry.Name,
                };

                // マウスを乗せたらフルのファイル名を表示する
                var imgTip = new System.Windows.Forms.ToolTip();
                imgTip.InitialDelay = 200;
                imgTip.SetToolTip(imgNameLabel, Path.GetFileName(entry.Name));

                imgNameLabel.MouseEnter += (sender, e) =>
                {
                    imgNameLabel.Parent.BackColor = FileSelectColor;
                };

                imgNameLabel.MouseLeave += (sender, e) =>
                {
                    imgNameLabel.Parent.BackColor = DefaultBackColor;
                };

                // 画像とファイル名を1セットにしたパネル
                var imgAndNameTableLayoutPanel = new TableLayoutPanel()
                {
                    AutoSize = true,
                    ColumnCount = 1,
                    RowCount = 2,
                    Anchor = AnchorStyles.Left,
                };

                imgAndNameTableLayoutPanel.MouseEnter += (sender, e) =>
                {
                    imgAndNameTableLayoutPanel.BackColor = FileSelectColor;
                };

                imgAndNameTableLayoutPanel.MouseLeave += (sender, e) =>
                {
                    imgAndNameTableLayoutPanel.BackColor = DefaultBackColor;
                };

                imgAndNameTableLayoutPanel.Controls.Add(p);
                imgAndNameTableLayoutPanel.Controls.Add(imgNameLabel);

                showImgFlowLayoutPanel.Controls.Add(imgAndNameTableLayoutPanel);
            }

            mainFlowLayoutPanel.Controls.Add(showImgFlowLayoutPanel);
        }

        /// <summary>
        /// 指定したファイル名の画像の次の画像を返す
        /// </summary>
        /// <returns></returns>
        public Image GetNextImg()
        {
            var nextIndex = imgNameList.IndexOf(nowShowingImg) + 1;
            if(imgNameList.Count <= nextIndex)
            {
                nowShowingImg = imgNameList[0];
                return imgDict[nowShowingImg];
            }

            nowShowingImg = imgNameList[nextIndex];
            return imgDict[nowShowingImg];
        }

        /// <summary>
        /// 指定したファイル名の画像の前の画像を返す
        /// </summary>
        /// <returns></returns>
        public Image GetPreviousImg()
        {
            var prevIndex = imgNameList.IndexOf(nowShowingImg) - 1;
            if(0 > prevIndex)
            {
                nowShowingImg = imgNameList[imgNameList.Count - 1];
                return imgDict[nowShowingImg];
            }

            nowShowingImg = imgNameList[prevIndex];
            return imgDict[nowShowingImg];
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
            //LoadZipFile(((string[])e.Data.GetData(DataFormats.FileDrop))[0]);
            foreach(var file in (string[])e.Data.GetData(DataFormats.FileDrop))
            {
                LoadZipFile(file);
            }
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            foreach (var p in splitLinePanelList)
            {
                p.Height = 1;
                p.Width = mainFlowLayoutPanel.Width - 7;
            }

            foreach(var p in showImgFlowLayoutPanelList)
            {
                p.MaximumSize = new Size(mainFlowLayoutPanel.Width - 7, 9999999);
            }
        }
    }
}
