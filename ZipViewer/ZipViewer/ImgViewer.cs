using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZipViewer
{
    public partial class ImgViewer : Form
    {
        private IImgController imgController;

        public ImgViewer(IImgController imgController)
        {
            InitializeComponent();

            this.imgController = imgController;
        }

        public void SetImage(Image img)
        {
            pictureBox.Image = img;
        }

        private void pictureBox_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ImgViewer_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Right)
            {
                SetImage(imgController.GetNextImg());
            }
            else if(e.KeyCode == Keys.Left)
            {
                SetImage(imgController.GetPreviousImg());
            }
        }
    }
}
