using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SchoolWebsite.Models
{
    public class YearFilter
    {
        [Required]
        public string year { get; set; }
    }
}