﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WootrixV2.Models
{
    public class CompanyTypeOfUser
    { 
        [Key]
        public int ID { get; set; }

        [ScaffoldColumn(false)]
        public int CompanyID { get; set; }

        [Required]
        [RegularExpression(@"^[^\|]+$", ErrorMessage = "Please no | characters")]
        [StringLength(1000)]
        [Display(Name = "Type Of User", Prompt = "Type Of User", Description = "Type Of User")]
        public string TypeOfUser { get; set; }
        
    }
}
