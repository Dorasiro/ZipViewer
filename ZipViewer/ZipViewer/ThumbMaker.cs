using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipViewer
{
    internal class ThumbMaker
    {
        /// <summary>
        /// 指定サイズのサムネを作る
        /// </summary>
        /// <param name="img">サムネ化する画像</param>
        /// <param name="width">幅</param>
        /// <param name="height">高さ</param>
        /// <returns></returns>
        public static Bitmap MakeThumb(Image img, int width, int height)
        {
            if(width <= 0 || height <= 0)
            {
                throw new ArgumentException("サイズ0以下は無理");
            }

            // 長い方を基準に縦横比を維持して縮小をかける
            if(img.Width > img.Height)
            {
                height = (int)(img.Height * ((double)width / (double)img.Width));
            }
            else
            {
                width = (int)(img.Width * ((double)height / (double)img.Height));
            }

            var bmp = new Bitmap(width, height);

            using (var g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(img, 0, 0, width, height);
            }
                
            return bmp;
        }
    }
}
