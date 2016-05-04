using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    public class PictureData
    {
        public List<PictureInfo> PictureList { get; set; }
        public String PictureUrlAddress { get; set; }
        public String PictureHeight { get; set; }
    }
}
