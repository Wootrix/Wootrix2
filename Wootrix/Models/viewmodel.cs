using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Web.WebPages.Html;

namespace WootrixV2.Models
{
    public class viewmodel
    {
        

        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [Display(Name = "Device Type")]
        public string DeviceType { get; set; }

        [Display(Name = "OS Type")]
        public string OSType { get; set; }

        [Display(Name = "Interface Language")]
        public string InterfaceLanguage { get; set; }
        public IEnumerable<SelectListItem> InterfaceLanguages { get; set; }
      

        [Display(Name = "Type Of User", Prompt = "Type Of User", Description = "Type Of User")]
        public string TypeOfUser { get; set; }
        public IList<string> SelectedTypeOfUser { get; set; }
        public IList<SelectListItem> AvailableTypeOfUser { get; set; }

        [Display(Name = "User Country", Prompt = "User Country", Description = "User Country")]
        public string Country { get; set; }
        public IEnumerable<SelectListItem> Countries { get; set; }


        [Display(Name = "State", Prompt = "State", Description = "State")]
        public string State { get; set; }
        public IEnumerable<SelectListItem> States { get; set; }

        [Display(Name = "City", Prompt = "City", Description = "City")]
        public string City { get; set; }
        public IEnumerable<SelectListItem> Cities { get; set; }

      
    }
}
