using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal.Models
{
    public class GeneralModel
    {
        public MenuModel MenuViewModel { get; set; }

        public List<WidgetDataModel> WidgetDataViewModel { get; set; }

        public ExtraModel ExtraViewModel { get; set; }

        public LocalizationModel LocalizationModel { get; set; }

        public ZoneModel ZoneModel { get; set; }

        public String PortalLogoURL { get; set; }
    }
}