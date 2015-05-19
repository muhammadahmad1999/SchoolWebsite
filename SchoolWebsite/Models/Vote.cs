using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SchoolWebsite.Models
{
    public class Vote
    {
        public int VoteID { get; set; }
        public int PollID { get; set; }
        public string UserID { get; set; }
        public string Answer { get; set; }
        public DateTime CastedAt { get; set; }
    }
}