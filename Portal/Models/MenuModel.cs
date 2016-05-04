using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal.Models
{
    public class MenuModel
    {
        public String IsVisible { get; set; }
        public String LoggedUser { get; set; }
        public List<Menu> Menus { get; set; }
        public List<LanguageModel> Languages { get; set; }
        public String Picture { get; set; }
        public String Scripts { get; set; }
        public String ComingFrom { get; set; }
        public String URL { get; set; }
        public String URLName { get; set; }
        public String UniqueName { get; set; }
    }
    public class Menu
    {
        public String ExternalLink { get; set; }
        public String Order { get; set; }
        public String Name { get; set; }
        public Boolean IsSubMenu { get; set; }
        public String ParentMenuId { get; set; }
        public String PageId { get; set; }
        public String MenuId { get; set; }
        public String URLName { get; set; }
        public String UniqueName { get; set; }
    }

   
   
}