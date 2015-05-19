using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SchoolWebsite.Models
{
    public class Poll
    {
        public int PollID { get; set; }
        public string Title { get; set; }
        public int NumQuestions { get; set; }
        public int NumAnswers { get; set; }

        public string questions { get; set; }
        public string answers { get; set; }
        public string CreatedBy { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime Limit { get; set; }
        public bool Active { get; set; }

        public string For { get; set; }
        public string Security { get; set; }
    }
}