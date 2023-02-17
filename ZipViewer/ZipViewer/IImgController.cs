using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipViewer
{
    public interface IImgController
    {
        Image GetNextImg();
        Image GetPreviousImg();
    }
}
