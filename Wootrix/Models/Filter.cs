using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WootrixV2.Models
{
    public class Filter
    {
        //[Required(ErrorMessage = "Date is required")]
       
        public string FromDate { get; set; }
        //[Required(ErrorMessage = "Date is required")]
      
        public string ToDate { get; set; }
        public string Name { get; set; }
    }
}
