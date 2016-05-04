using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    public class GridRowData
    {
       
        public List<GridData> Data { get; set; }

       
        public String RowNumber { get; set; }

       
        public String HasAction { get; set; }

       
        public String IsEmptyGrid { get; set; }

       
        public String Width { get; set; }

       
        public String TotalRecord { get; set; }
    }
}
