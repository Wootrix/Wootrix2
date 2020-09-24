using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WootrixV2.Models
{
    public class Reporting
    {
        public IEnumerable<WootrixV2.Models.ArticleReporting> Reports { get; set; }

        public FilterData FilterDetaails { get; set; }
        public List<viewmodel> ViewFilterData { get; set; }
    

        public Reporting()
        {
            FilterDetaails =new FilterData();
        }
    }
}
