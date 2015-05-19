using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SchoolWebsite.Models
{
    public class HousePoint
    {
        public int HousePointID { get; set; }

        public string AwardedTo { get; set; }

        public DateTime DateAwarded { get; set; }
        public string AwardingTeacher { get; set; }
        public string Reason { get; set; }
    }
}