using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SchoolWebsite.Models
{
    public class PollDetail
    {
        public Array answers { get; set; }
        public List<string> questions { get; set; }

        public Poll poll { get; set; }
    }

    public class PollResults
    {
        public Array answers { get; set; }
        public string answer_cluster { get; set; }
        public Array votes { get; set; }
        public int TotalVote { get; set; }
        public List<string> questions { get; set; }

        public Poll poll { get; set; }
    }
}