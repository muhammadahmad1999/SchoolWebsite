using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SchoolWebsite.Models
{
    public class Config
    {
        public int ConfigID { get; set; }
        public string SystemID { get; set; }
        public string Action { get; set; }
        public string RolesAllowed { get; set; }
    }
}