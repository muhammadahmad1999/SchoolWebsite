using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using SchoolWebsite.Models;


namespace SchoolWebsite.Controllers
{
    [Authorize()]
    public class PollsController : Controller
    {
        public string SystemID = "PollingSystem";

        private SchoolDb db = new SchoolDb();
        ApplicationDbContext context = new ApplicationDbContext();
        
        private const string seperator_key = ";";

        public List<string> GetRolesAllowed(string action)
        {
            List<string> roles = new List<string>();
            //roles.Add("Administrator");
            //roles.Add("Prefect");

            string role_str = db.Configs.FirstOrDefault(a => a.SystemID == SystemID && a.Action == action).RolesAllowed;

            if (role_str != null)
            {
                string[] split_data = role_str.Split(';');
                return split_data.ToList();
            }
            else
            {
                return new List<string>();
            }
        }

        public List<string> GetRolesAllowed(string SystemID, string action)
        {
            List<string> roles = new List<string>();
            //roles.Add("Administrator");
            //roles.Add("Prefect");

            string role_str = db.Configs.FirstOrDefault(a => a.SystemID == SystemID && a.Action == action).RolesAllowed;


            if (role_str != null)
            {
                string[] split_data = role_str.Split(';');
                return split_data.ToList();
            }
            else
            {
                return new List<string>();
            }
            
        }

        public bool HasUserVoted(int id, string user_name)
        {
            //db.SaveChanges();
            //db = new SchoolDb();

            var votes = db.Votes.ToArray();

            var check_vote = votes
                .Where(d => d.PollID == id)
                .Where(d => d.UserID == user_name)
                .Select(d => d.UserID).SingleOrDefault();

            if(check_vote != null)
            {
                return true;
            }

            return false;
        }

        // GET: Polls
        public ActionResult Index()
        {
             ApplicationUser user = context.Users.Find(User.Identity.GetUserId());

            db.Polls.Include(@poll => @poll.Title);
            //db.Polls.Include(@poll => @poll.Title).Include(@poll => @poll.CreatedOn).Include(@poll => @poll.Limit);
           // return View(db.Polls.ToList());

            string user_name = User.Identity.GetUserName().ToString();

            //db.Dispose();
            db = new SchoolDb();

            List<Poll> polls = new List<Poll>();
            foreach(Poll x in db.Polls)
            {
                if (x.For != null)
                {
                    if (x.For != "")
                    {
                        if (!x.For.Contains("All"))
                        {
                            if (!x.For.Contains(';'))
                            {
                                if ((x.For.Remove(0, x.For.IndexOf(":") + 1) == user.Year ||
                                    x.For.Remove(0, 3) == "All" ||
                                    x.For.Remove(0, x.For.IndexOf(":") + 1) == user.TutorGroup)
                                    &&
                                    !HasUserVoted(x.PollID, user_name))
                                {
                                    polls.Add(x);
                                }
                            }
                            else
                            {
                                List<string> array = x.For.Split(';').ToList();

                                foreach (string y in array)
                                {
                                    if ((y.Remove(0, y.IndexOf(":") + 1) == user.Year ||
                                     y.Remove(0, y.IndexOf(":") + 1) == user.TutorGroup)
                                    &&
                                    !HasUserVoted(x.PollID, user_name))
                                    {
                                        polls.Add(x);
                                    }
                                }
                            }
                        }
                        else if (!HasUserVoted(x.PollID, user_name))
                        {
                            polls.Add(x);
                        }
                    }
                }
                    //get rid of this last statement - only for development mode
               //else
               // {
               //     polls.Add(x);
               // }

                //db.Dispose();
                db = new SchoolDb();
            }

            return View(polls);
        }

        public ActionResult PollHistory()
        {
            string action = "ViewAllPolls";

            ApplicationUser user = context.Users.Find(User.Identity.GetUserId());

            db.Polls.Include(@poll => @poll.Title);
            //db.Polls.Include(@poll => @poll.Title).Include(@poll => @poll.CreatedOn).Include(@poll => @poll.Limit);
            // return View(db.Polls.ToList());

            bool can_view_all_polls = false;

            List<string> roles = GetRolesAllowed(action);

            foreach (string x in roles)
            {
                if (User.IsInRole(x))
                {
                    can_view_all_polls = true;
                }
            }

            List<Poll> polls = new List<Poll>();
            foreach (Poll x in db.Polls)
            {
                if (!can_view_all_polls)
                {
                    if (x.CreatedBy == user.UserName)
                    {
                        polls.Add(x);
                    }
                }
                else
                {
                    polls.Add(x);
                }
            }

            return View(polls);
        }

        [HttpGet]
        public ActionResult Participate(int? id)
        {
            ApplicationUser user = context.Users.Find(User.Identity.GetUserId());

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if(db.Polls.Find(id) == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            

            string user_name = User.Identity.GetUserName().ToString();
            var check_vote = db.Votes
                .Where(d => d.PollID == id)
                .Where(d => d.UserID == user_name)
                .Select(d => d.UserID).SingleOrDefault();

            if (check_vote == null)
            {
                List<string> answers = new List<string>();
                string answer_cluster = db.Polls.Find(id).answers;
                int NumAnswers = db.Polls.Find(id).NumAnswers;

                for (int i = 0; i < NumAnswers; i++)
                {
                    string b = answer_cluster.Substring(0, answer_cluster.IndexOf(";"));
                    answer_cluster = answer_cluster.Substring(answer_cluster.IndexOf(";") + 1, answer_cluster.Length - (answer_cluster.IndexOf(";") + 1));
                    string ans;
                    ans = b;

                    answers.Add(ans);
                }

                List<string> questions = new List<string>();
                string question_cluster = db.Polls.Find(id).questions;
                int NumQuestions = db.Polls.Find(id).NumQuestions;

                for (int i = 0; i < NumQuestions; i++)
                {
                    string b = question_cluster.Substring(0, question_cluster.IndexOf(";"));
                    question_cluster = question_cluster.Substring(question_cluster.IndexOf(";") + 1, question_cluster.Length - (question_cluster.IndexOf(";") + 1));
                    string question;
                    question = b;

                    questions.Add(question);
                }

                Poll poll = db.Polls.Find(id);
                if (poll == null)
                {
                    return HttpNotFound();
                }

                bool can_participate = false;

                if (poll.For != null)
                {
                    if (poll.For != "")
                    {
                        /*
                        if (poll.For.Remove(0, poll.For.IndexOf(":") + 1) == user.Year ||
                            poll.For == "All" || poll.For.Remove(0, poll.For.IndexOf(":") + 1) == user.TutorGroup)
                        {
                            can_participate = true;
                        }
                        */

                        if (!poll.For.Contains("All"))
                        {
                            if (!poll.For.Contains(';'))
                            {
                                if ((poll.For.Remove(0, poll.For.IndexOf(":") + 1) == user.Year ||
                                    poll.For.Remove(0, poll.For.IndexOf(":") + 1) == user.TutorGroup)
                                    &&
                                    !HasUserVoted(poll.PollID, user_name))
                                {
                                    can_participate = true;
                                }
                            }
                            else
                            {
                                List<string> array = poll.For.Split(';').ToList();

                                foreach (string y in array)
                                {
                                    if ((y.Remove(0, y.IndexOf(":") + 1) == user.Year ||
                                    y.Remove(0, y.IndexOf(":") + 1) == user.TutorGroup)
                                    &&
                                    !HasUserVoted(poll.PollID, user_name))
                                    {
                                        can_participate = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            can_participate = true;
                        }
                    }
                }
 
                

                ViewBag.can_participate = can_participate;
                ViewBag.MaxAnswers = 10;

               // answers.Add("null");

                PollDetail poll_detail = new PollDetail();
                poll_detail.answers = answers.ToArray();
                poll_detail.questions = questions;
                poll_detail.poll = poll;

                ViewBag.Allowed = true;

                return View(poll_detail);
            }
            else
            {
                PollDetail poll_detail = new PollDetail();
                poll_detail.answers = new List<string>().ToArray();
                poll_detail.questions = new List<string>();
                poll_detail.poll = new Poll();

                ViewBag.Allowed = false;
                ViewBag.can_participate = true;
                ModelState.AddModelError("Error", "You have already voted!");
                return View(poll_detail);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Participate()
        {
            Vote vote = new Vote();

            string answer = "";
            int num_ques = Convert.ToInt16(Request["num_ques"]);
            

            for(int i = 0; i < num_ques; i++)
            {
                
                string a = Request["answers" + i.ToString()];

                if (a == null)
                {
                    int num_other = Convert.ToInt16(Request["num_other_ques" + i.ToString()]);
                    for(int j = 0; j < num_other; j++)
                    {
                        string b = Request["Other" + j.ToString()];

                        answer += b + ";";
                    }
                }
                else
                {
                    answer += "check" + (Convert.ToInt16(a) + 1).ToString() + ";";
                }

                answer += "null" + ";";
            }

            vote.Answer = answer;
            vote.PollID = Convert.ToInt16(Request["pollID"]);
            vote.UserID = User.Identity.GetUserName().ToString();
            vote.CastedAt = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.Votes.Add(vote);

                db.SaveChanges();
                return Redirect("/polls/Voting_ThankYou/" + vote.PollID.ToString());
            }

            return View();
        }

        public ActionResult Voting_ThankYou(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if(db.Polls.Find(id) == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Poll poll = db.Polls.Find(id);

            return View(poll);
        }

        [HttpGet]
        //[Authorize(Roles="Administrator")]
        public ActionResult ResultsSimple(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (db.Polls.Find(id) == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            PollResults results = new PollResults();

            List<string> answers = new List<string>();
            string answer_cluster = db.Polls.Find(id).answers;

            results.answer_cluster = answer_cluster;

            int NumAnswers = db.Polls.Find(id).NumAnswers;

            for (int i = 0; i < NumAnswers; i++)
            {
                string b = answer_cluster.Substring(0, answer_cluster.IndexOf(";"));
                
                string ans;
                ans = b;

                answers.Add(ans);

                //prepare for next loop (to find right ";") - remove
                answer_cluster = answer_cluster.Remove(0, answer_cluster.IndexOf(";") + 1);
            }

            List<string> questions = new List<string>();
            string question_cluster = db.Polls.Find(id).questions;
            int NumQuestions = db.Polls.Find(id).NumQuestions;

            for (int i = 0; i < NumQuestions; i++)
            {
                string b = question_cluster.Substring(0, question_cluster.IndexOf(";"));
                question_cluster = question_cluster.Substring(question_cluster.IndexOf(";") + 1, question_cluster.Length - (question_cluster.IndexOf(";") + 1));
                string question;
                question = b;

                questions.Add(question);
            }

            var votes = db.Votes
                .Where(d => d.PollID == id)
                .Select(d => d.Answer);

            var votes_cluster = votes.ToArray();

            List<string> votes_count_str = new List<string>();

            var y = db.Polls.Find(id).answers;
            var answer_index = 1;

            for(int j = answer_index; j < NumAnswers; j++)
            {
                j = answer_index - 1;
                if(y == "")
                    break;
                
                if (y.Substring(0, y.IndexOf(";")) == "null")
                {
                    votes_count_str.Add("null");

                    y = y.Remove(0, y.IndexOf(";") + 1);

                    int u = 0;
                    foreach (var x in votes_cluster)
                    {
                        var z = x.Remove(0, x.IndexOf(";"));
                        
                        //can add if statement
                        if(NumQuestions > 1)
                            z = z.Remove(0, x.IndexOf(";"));

                        votes_cluster.SetValue(z, u);
                        u++;
                    }

                    j = 0;
                    answer_index = 0;
                }
                else
                {
                    int num_votes = 0;
                    string loc = "check" + (j + 1).ToString();

                    //check2;null;check1;null;


                    foreach (var x in votes_cluster)
                    {
                        string check = x.Substring(0, x.IndexOf(";"));
                        if (check == loc)
                            num_votes++;
                    }

                    votes_count_str.Add(num_votes.ToString());

                    y = y.Remove(0, y.IndexOf(";") + 1);
                }

                answer_index++;
        }
           
            results.answers = answers.ToArray();
            results.questions = questions;
            results.votes = votes_count_str.ToArray();
            results.TotalVote = votes.Count();

            results.poll = db.Polls.Find(id);

            return View(results);
        }

        [HttpGet]
        //[Authorize(Roles="Administrator")]
        public ActionResult Results(int ?id)
        {
            ViewBag.error = false;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (db.Polls.Find(id) == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            PollResults results = new PollResults();

            List<string> answers = new List<string>();
            string answer_cluster = db.Polls.Find(id).answers;

            results.answer_cluster = answer_cluster;

            int NumAnswers = db.Polls.Find(id).NumAnswers;

            for (int i = 0; i < NumAnswers; i++)
            {
                string b = answer_cluster.Substring(0, answer_cluster.IndexOf(";"));

                string ans;
                ans = b;

                answers.Add(ans);

                //prepare for next loop (to find right ";") - remove
                answer_cluster = answer_cluster.Remove(0, answer_cluster.IndexOf(";") + 1);
            }

            List<string> questions = new List<string>();
            string question_cluster = db.Polls.Find(id).questions;
            int NumQuestions = db.Polls.Find(id).NumQuestions;

            for (int i = 0; i < NumQuestions; i++)
            {
                string b = question_cluster.Substring(0, question_cluster.IndexOf(";"));
                question_cluster = question_cluster.Substring(question_cluster.IndexOf(";") + 1, question_cluster.Length - (question_cluster.IndexOf(";") + 1));
                string question;
                question = b;

                questions.Add(question);
            }

            var votes = db.Votes
                .Where(d => d.PollID == id)
                .Select(d => d.Answer);

            int num = votes.Count();

            var votes_cluster = votes.ToArray();

            List<string> votes_count_str = new List<string>();

            var y = db.Polls.Find(id).answers;
            var answer_index = 1;

            for (int j = answer_index; j < NumAnswers; j++)
            {
                j = answer_index - 1;
                if (y == "")
                    break;

                if (y.Substring(0, y.IndexOf(";")) == "null")
                {
                    votes_count_str.Add("null");

                    y = y.Remove(0, y.IndexOf(";") + 1);

                    int u = 0;
                    foreach (var x in votes_cluster)
                    {
                        var z = x.Remove(0, x.IndexOf(";"));

                        //can add if statement
                        if (NumQuestions > 1)
                            z = z.Remove(0, x.IndexOf(";"));

                        votes_cluster.SetValue(z, u);
                        u++;
                    }

                    j = 0;
                    answer_index = 0;
                }
                else
                {
                    int num_votes = 0;
                    string loc = "check" + (j + 1).ToString();

                    //check2;null;check1;null;


                    foreach (var x in votes_cluster)
                    {
                        string check = x.Substring(0, x.IndexOf(";"));
                        if (check == loc)
                            num_votes++;
                    }

                    votes_count_str.Add(num_votes.ToString());

                    y = y.Remove(0, y.IndexOf(";") + 1);
                }

                answer_index++;
            }

            results.answers = answers.ToArray();
            results.questions = questions;
            results.votes = votes_count_str.ToArray();
            results.TotalVote = votes.Count();

            results.poll = db.Polls.Find(id);

            ViewBag.results = results;

            ViewBag.roles = GetRolesAllowed("StudentSearchSystem", "SearchStudents");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResultsByYear(string year, int id)
        {
            ViewBag.error = false;
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (year == "")
            {
                ViewBag.year_error = "The year field is empty!";
                ViewBag.error = true;
                return View("results");
                //return View("Results");
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (db.Polls.Find(id) == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            
                PollResults results = new PollResults();

                List<string> answers = new List<string>();
                string answer_cluster = db.Polls.Find(id).answers;

                results.answer_cluster = answer_cluster;

                int NumAnswers = db.Polls.Find(id).NumAnswers;

                for (int i = 0; i < NumAnswers; i++)
                {
                    string b = answer_cluster.Substring(0, answer_cluster.IndexOf(";"));

                    string ans;
                    ans = b;

                    answers.Add(ans);

                    //prepare for next loop (to find right ";") - remove
                    answer_cluster = answer_cluster.Remove(0, answer_cluster.IndexOf(";") + 1);
                }

                List<string> questions = new List<string>();
                string question_cluster = db.Polls.Find(id).questions;
                int NumQuestions = db.Polls.Find(id).NumQuestions;

                for (int i = 0; i < NumQuestions; i++)
                {
                    string b = question_cluster.Substring(0, question_cluster.IndexOf(";"));
                    question_cluster = question_cluster.Substring(question_cluster.IndexOf(";") + 1, question_cluster.Length - (question_cluster.IndexOf(";") + 1));
                    string question;
                    question = b;

                    questions.Add(question);
                }

                var votes = db.Votes
                    .Where(d => d.PollID == id)
                    .Select(d => d.Answer);

                var users = db.Votes
                    .Where(d => d.PollID == id)
                    .Select(d => d.UserID);

                var votes_cluster_array = votes.ToArray();
                var users_cluster = users.ToArray();

                for (int i = 0; i < votes.Count(); i++)
                {
                    string username = users_cluster.GetValue(i).ToString();

                    ApplicationUser user = context.Users.Where(u => u.UserName.Equals(username, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();

                    if (user.Year != null)
                    {
                        if (user.Year == year)
                        {

                        }
                        else if (votes_cluster_array != null)
                        {
                            votes_cluster_array.SetValue(null, i);
                        }
                    }
                }

                var votes_cluster = votes_cluster_array;

                List<string> votes_count_str = new List<string>();

                var y = db.Polls.Find(id).answers;
                var answer_index = 1;

                for (int j = answer_index; j < NumAnswers; j++)
                {
                    j = answer_index - 1;
                    if (y == "")
                        break;

                    if (y.Substring(0, y.IndexOf(";")) == "null")
                    {
                        votes_count_str.Add("null");

                        y = y.Remove(0, y.IndexOf(";") + 1);

                        int u = 0;
                        foreach (var x in votes_cluster_array)
                        {
                            if (x != "" && x != null)
                            {
                                var z = x.Remove(0, x.IndexOf(";"));

                                //can add if statement
                                if (NumQuestions > 1)
                                    z = z.Remove(0, x.IndexOf(";"));

                                votes_cluster[u] = z;
                            }
                            u++;
                        }

                        j = 0;
                        answer_index = 0;
                    }
                    else
                    {
                        int num_votes = 0;
                        string loc = "check" + (j + 1).ToString();

                        //check2;null;check1;null;


                        foreach (var x in votes_cluster)
                        {
                            if (x != null && x != "")
                            {
                                string check = x.Substring(0, x.IndexOf(";"));
                                if (check == loc)
                                    num_votes++;
                            }
                        }

                        votes_count_str.Add(num_votes.ToString());

                        y = y.Remove(0, y.IndexOf(";") + 1);
                    }

                    answer_index++;
                }

                results.answers = answers.ToArray();
                results.questions = questions;
                results.votes = votes_count_str.ToArray();
                results.TotalVote = votes.Count();

                results.poll = db.Polls.Find(id);

                ViewBag.results = results;

                ViewBag.roles = GetRolesAllowed("StudentSearchSystem", "SearchStudents");

                return View("Results");

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResultsByName(string q, int id)
        {
            ViewBag.error = false;
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (q == "")
            {
                ViewBag.name_error = "The name field is empty!";
                ViewBag.error = true;
                return View("results");
                //return View("Results");
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (db.Polls.Find(id) == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }


            PollResults results = new PollResults();

            List<string> answers = new List<string>();
            string answer_cluster = db.Polls.Find(id).answers;

            results.answer_cluster = answer_cluster;

            int NumAnswers = db.Polls.Find(id).NumAnswers;

            for (int i = 0; i < NumAnswers; i++)
            {
                string b = answer_cluster.Substring(0, answer_cluster.IndexOf(";"));

                string ans;
                ans = b;

                answers.Add(ans);

                //prepare for next loop (to find right ";") - remove
                answer_cluster = answer_cluster.Remove(0, answer_cluster.IndexOf(";") + 1);
            }

            List<string> questions = new List<string>();
            string question_cluster = db.Polls.Find(id).questions;
            int NumQuestions = db.Polls.Find(id).NumQuestions;

            for (int i = 0; i < NumQuestions; i++)
            {
                string b = question_cluster.Substring(0, question_cluster.IndexOf(";"));
                question_cluster = question_cluster.Substring(question_cluster.IndexOf(";") + 1, question_cluster.Length - (question_cluster.IndexOf(";") + 1));
                string question;
                question = b;

                questions.Add(question);
            }

            var votes = db.Votes
                .Where(d => d.PollID == id)
                .Select(d => d.Answer);

            var users = db.Votes
                .Where(d => d.PollID == id)
                .Select(d => d.UserID);

            var votes_cluster_array = votes.ToArray();
            var users_cluster = users.ToArray();

            for (int i = 0; i < votes.Count(); i++)
            {
                string username = users_cluster.GetValue(i).ToString();

                ApplicationUser user = context.Users.Where(u => u.UserName.Equals(username, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();

                //string name = user.Name;
                //bool contain = name.Contains(q.ToString());
                
                //if (user.Name.Contains(q.ToString()))
                //{

               // }
                if (user.Name != null)
                {
                    if (user.Name.IndexOf(q, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
                    {

                    }
                    else if (votes_cluster_array != null)
                    {
                        votes_cluster_array.SetValue("", i);
                    }
                }
            }

            var votes_cluster = votes_cluster_array;

            List<string> votes_count_str = new List<string>();

            var y = db.Polls.Find(id).answers;
            var answer_index = 1;

            for (int j = answer_index; j < NumAnswers; j++)
            {
                j = answer_index - 1;
                if (y == "")
                    break;

                if (y.Substring(0, y.IndexOf(";")) == "null")
                {
                    votes_count_str.Add("null");

                    y = y.Remove(0, y.IndexOf(";") + 1);

                    int u = 0;
                    foreach (var x in votes_cluster)
                    {
                        if (x != "")
                        {
                            var z = x.Remove(0, x.IndexOf(";"));

                            //can add if statement
                            if (NumQuestions > 1)
                                z = z.Remove(0, x.IndexOf(";"));

                            votes_cluster.SetValue(z, u);
                            
                        }
                        u++;
                    }

                    j = 0;
                    answer_index = 0;
                }
                else
                {
                    int num_votes = 0;
                    string loc = "check" + (j + 1).ToString();

                    //check2;null;check1;null;


                    if (votes_cluster != null)
                    {
                        foreach (var x in votes_cluster)
                        {
                            if (x != "")
                            {
                                string check = x.Substring(0, x.IndexOf(";"));
                                if (check == loc)
                                    num_votes++;
                            }
                        }
                    }

                    votes_count_str.Add(num_votes.ToString());

                    y = y.Remove(0, y.IndexOf(";") + 1);
                }

                answer_index++;
            }

            results.answers = answers.ToArray();
            results.questions = questions;
            results.votes = votes_count_str.ToArray();
            results.TotalVote = votes.Count();

            results.poll = db.Polls.Find(id);

            ViewBag.results = results;

            ViewBag.roles = GetRolesAllowed("StudentSearchSystem", "SearchStudents");

            return View("Results");

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResultsByTutorGroup(string q, int id)
        {
            ViewBag.error = false;
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (q == "")
            {
                ViewBag.tutorgroup_error = "The tutor group field is empty!";
                ViewBag.error = true;
                return View("results");
                //return View("Results");
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (db.Polls.Find(id) == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }


            PollResults results = new PollResults();

            List<string> answers = new List<string>();
            string answer_cluster = db.Polls.Find(id).answers;

            results.answer_cluster = answer_cluster;

            int NumAnswers = db.Polls.Find(id).NumAnswers;

            for (int i = 0; i < NumAnswers; i++)
            {
                string b = answer_cluster.Substring(0, answer_cluster.IndexOf(";"));

                string ans;
                ans = b;

                answers.Add(ans);

                //prepare for next loop (to find right ";") - remove
                answer_cluster = answer_cluster.Remove(0, answer_cluster.IndexOf(";") + 1);
            }

            List<string> questions = new List<string>();
            string question_cluster = db.Polls.Find(id).questions;
            int NumQuestions = db.Polls.Find(id).NumQuestions;

            for (int i = 0; i < NumQuestions; i++)
            {
                string b = question_cluster.Substring(0, question_cluster.IndexOf(";"));
                question_cluster = question_cluster.Substring(question_cluster.IndexOf(";") + 1, question_cluster.Length - (question_cluster.IndexOf(";") + 1));
                string question;
                question = b;

                questions.Add(question);
            }

            var votes = db.Votes
                .Where(d => d.PollID == id)
                .Select(d => d.Answer);

            var users = db.Votes
                .Where(d => d.PollID == id)
                .Select(d => d.UserID);

            var votes_cluster_array = votes.ToArray();
            var users_cluster = users.ToArray();

            for (int i = 0; i < votes.Count(); i++)
            {
                string username = users_cluster.GetValue(i).ToString();

                ApplicationUser user = context.Users.Where(u => u.UserName.Equals(username, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();

                //string name = user.Name;
                //bool contain = name.Contains(q.ToString());

                //if (user.Name.Contains(q.ToString()))
                //{

                // }
                if (user.TutorGroup != null)
                {
                    if (user.TutorGroup.IndexOf(q, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
                    {

                    }
                    else if (votes_cluster_array != null)
                    {
                        votes_cluster_array.SetValue("", i);
                    }
                }
            }

            var votes_cluster = votes_cluster_array;

            List<string> votes_count_str = new List<string>();

            var y = db.Polls.Find(id).answers;
            var answer_index = 1;

            for (int j = answer_index; j < NumAnswers; j++)
            {
                j = answer_index - 1;
                if (y == "")
                    break;

                if (y.Substring(0, y.IndexOf(";")) == "null")
                {
                    votes_count_str.Add("null");

                    y = y.Remove(0, y.IndexOf(";") + 1);

                    int u = 0;
                    foreach (var x in votes_cluster)
                    {
                        if (x != "")
                        {
                            var z = x.Remove(0, x.IndexOf(";"));

                            //can add if statement
                            if (NumQuestions > 1)
                                z = z.Remove(0, x.IndexOf(";"));

                            votes_cluster.SetValue(z, u);
                           
                        }
                        u++;
                    }

                    j = 0;
                    answer_index = 0;
                }
                else
                {
                    int num_votes = 0;
                    string loc = "check" + (j + 1).ToString();

                    //check2;null;check1;null;


                    if (votes_cluster != null)
                    {
                        foreach (var x in votes_cluster)
                        {
                            if (x != "")
                            {
                                string check = x.Substring(0, x.IndexOf(";"));
                                if (check == loc)
                                    num_votes++;
                            }
                        }
                    }

                    votes_count_str.Add(num_votes.ToString());

                    y = y.Remove(0, y.IndexOf(";") + 1);
                }

                answer_index++;
            }

            results.answers = answers.ToArray();
            results.questions = questions;
            results.votes = votes_count_str.ToArray();
            results.TotalVote = votes.Count();

            results.poll = db.Polls.Find(id);

            ViewBag.results = results;

            ViewBag.roles = GetRolesAllowed("StudentSearchSystem", "SearchStudents");

            return View("Results");

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResultsByUsername(string q, int id)
        {
            ViewBag.error = false;
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (q == "")
            {
                ViewBag.username_error = "The username field is empty!";
                ViewBag.error = true;
                return View("results");
                //return View("Results");
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (db.Polls.Find(id) == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }


            PollResults results = new PollResults();

            List<string> answers = new List<string>();
            string answer_cluster = db.Polls.Find(id).answers;

            results.answer_cluster = answer_cluster;

            int NumAnswers = db.Polls.Find(id).NumAnswers;

            for (int i = 0; i < NumAnswers; i++)
            {
                string b = answer_cluster.Substring(0, answer_cluster.IndexOf(";"));

                string ans;
                ans = b;

                answers.Add(ans);

                //prepare for next loop (to find right ";") - remove
                answer_cluster = answer_cluster.Remove(0, answer_cluster.IndexOf(";") + 1);
            }

            List<string> questions = new List<string>();
            string question_cluster = db.Polls.Find(id).questions;
            int NumQuestions = db.Polls.Find(id).NumQuestions;

            for (int i = 0; i < NumQuestions; i++)
            {
                string b = question_cluster.Substring(0, question_cluster.IndexOf(";"));
                question_cluster = question_cluster.Substring(question_cluster.IndexOf(";") + 1, question_cluster.Length - (question_cluster.IndexOf(";") + 1));
                string question;
                question = b;

                questions.Add(question);
            }

            var votes = db.Votes
                .Where(d => d.PollID == id)
                .Select(d => d.Answer);

            var users = db.Votes
                .Where(d => d.PollID == id)
                .Select(d => d.UserID);

            var votes_cluster_array = votes.ToArray();
            var users_cluster = users.ToArray();

            for (int i = 0; i < votes.Count(); i++)
            {
                string username = users_cluster.GetValue(i).ToString();

                ApplicationUser user = context.Users.Where(u => u.UserName.Equals(username, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();

                //string name = user.Name;
                //bool contain = name.Contains(q.ToString());

                //if (user.Name.Contains(q.ToString()))
                //{

                // }
                if (user.UserName != null)
                {
                    if (user.UserName.IndexOf(q, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
                    {

                    }
                    else if (votes_cluster_array != null)
                    {
                        votes_cluster_array.SetValue("", i);
                    }
                }
            }

            var votes_cluster = votes_cluster_array;

            List<string> votes_count_str = new List<string>();

            var y = db.Polls.Find(id).answers;
            var answer_index = 1;

            for (int j = answer_index; j < NumAnswers; j++)
            {
                j = answer_index - 1;
                if (y == "")
                    break;

                if (y.Substring(0, y.IndexOf(";")) == "null")
                {
                    votes_count_str.Add("null");

                    y = y.Remove(0, y.IndexOf(";") + 1);

                    int u = 0;
                    foreach (var x in votes_cluster)
                    {
                        if (x != "")
                        {
                            var z = x.Remove(0, x.IndexOf(";"));

                            //can add if statement
                            if (NumQuestions > 1)
                                z = z.Remove(0, x.IndexOf(";"));

                            votes_cluster.SetValue(z, u);
                           
                        }
                        u++;
                    }

                    j = 0;
                    answer_index = 0;
                }
                else
                {
                    int num_votes = 0;
                    string loc = "check" + (j + 1).ToString();

                    //check2;null;check1;null;


                    if (votes_cluster != null)
                    {
                        foreach (var x in votes_cluster)
                        {
                            if (x != "")
                            {
                                string check = x.Substring(0, x.IndexOf(";"));
                                if (check == loc)
                                    num_votes++;
                            }
                        }
                    }

                    votes_count_str.Add(num_votes.ToString());

                    y = y.Remove(0, y.IndexOf(";") + 1);
                }

                answer_index++;
            }

            results.answers = answers.ToArray();
            results.questions = questions;
            results.votes = votes_count_str.ToArray();
            results.TotalVote = votes.Count();

            results.poll = db.Polls.Find(id);

            ViewBag.results = results;

            ViewBag.roles = GetRolesAllowed("StudentSearchSystem", "SearchStudents");

            return View("Results");

        }

        // GET: Polls/Details/5
        public ActionResult Details(int? id)
        {
            //have null in string to tell the loop that the answers for the
            //previous question have ended

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            List<string> answers = new List<string>();
            string answer_cluster = db.Polls.Find(id).answers;
            int NumAnswers = db.Polls.Find(id).NumAnswers;

            for (int i = 0; i < NumAnswers; i++)
            {
                string b = answer_cluster.Substring(0, answer_cluster.IndexOf(";"));
                answer_cluster = answer_cluster.Substring(answer_cluster.IndexOf(";") + 1, answer_cluster.Length - (answer_cluster.IndexOf(";") + 1));
                string ans;
                ans = b;

                answers.Add(ans);
            }

            List<string> questions = new List<string>();
            string question_cluster = db.Polls.Find(id).questions;
            int NumQuestions = db.Polls.Find(id).NumQuestions;

            for (int i = 0; i < NumQuestions; i++)
            {
                string b = question_cluster.Substring(0, question_cluster.IndexOf(";"));
                question_cluster = question_cluster.Substring(question_cluster.IndexOf(";") + 1, question_cluster.Length - (question_cluster.IndexOf(";") + 1));
                string question;
                question = b;

                questions.Add(question);
            }

            Poll poll = db.Polls.Find(id);
            if (poll == null)
            {
                return HttpNotFound();
            }

            ViewBag.MaxAnswers = 10;

            PollDetail poll_detail = new PollDetail();
            poll_detail.answers = answers.ToArray();
            poll_detail.questions = questions;
            poll_detail.poll = poll;

            ViewBag.Null = poll_detail.answers.GetValue(2);

            return View(poll_detail);   
        }

        // GET: Polls/Create
        //[AuthorizeRedirect(Roles = "Administrator")]
        public ActionResult Create()
        {
            string action = "Create";
            //ApplicationUser user = context.Users.Find(User.Identity.GetUserId());


            ViewBag.roles = GetRolesAllowed(action);
            ViewBag.roles_07 = GetRolesAllowed(action + "07");
            ViewBag.roles_08 = GetRolesAllowed(action + "08");
            ViewBag.roles_09 = GetRolesAllowed(action + "09");
            ViewBag.roles_10 = GetRolesAllowed(action + "10");
            ViewBag.roles_11 = GetRolesAllowed(action + "11");
            ViewBag.roles_all = GetRolesAllowed(action + "All");
            ViewBag.roles_tutor_group = GetRolesAllowed(action + "TutorGroup");

            return View();
        }

        // POST: Polls/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "PollID,Title,NumQuestions,NumAnswers,questions,answers,CreatedOn,Limit")] Poll poll,
            IEnumerable<string> list)
        {
            ApplicationUser user = context.Users.Find(User.Identity.GetUserId());

            string poll_target = Request["list"].ToString();

            poll.For = "";

            string for_year = "";

            // if (User.IsInRole("Administrator"))
            //{
            foreach (string x in list)
            {
                if (x == "All")
                {
                    poll.For += x;

                    break;
                }
                else
                {
                    if (x == "TutorGroup")
                    {
                        poll.For += ";" + "Form:" + user.TutorGroup;
                    }
                    if (x.Substring(0, 4) == "Year")
                    {
                        for_year += "," +  x.Remove(0, x.IndexOf(":") + 1);
                    }
                }
            }

            if (for_year != "")
            {
                for_year = for_year.Remove(0, 1);

                poll.For += ";" + "Year:" + for_year;
            }

            poll.CreatedBy = User.Identity.GetUserName();

            poll.Active = true;

            poll.questions = "";
            poll.answers = "";

            DateTime BritishTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
                DateTime.UtcNow, "GMT Standard Time");

            poll.CreatedOn = BritishTime;
            poll.Limit = DateTime.Now;

            int num_questions = Convert.ToInt16(Request["num_questions"]);

            poll.NumQuestions = num_questions;

            for (int i = 0; i < num_questions; i++)
            {
                var question_id = "QuestionBox" + i.ToString();

                var question = Request[question_id];
                poll.questions += question + seperator_key;

                var num_answers_id = "num_answers" + i.ToString();
                var num_answers = Convert.ToInt16(Request[num_answers_id]);

                poll.NumAnswers += num_answers;

                for(int j = 0; j < num_answers; j++)
                {
                    var answer_id = "AnswerBox" + i.ToString() + j.ToString();
                    var answer = Request[answer_id];

                    var other_id = "Other" + i.ToString() + j.ToString();
                    var other = Request[other_id];

                    if(answer == null)
                    {
                        other = "other";
                    }
                    else
                    {
                        answer = "check" + answer;
                    }
                    

                    poll.answers += answer + other + seperator_key;
                }

                poll.answers += "null" + seperator_key;
                poll.NumAnswers += 1;
            }

                if (ModelState.IsValid)
                {

                    db.Polls.Add(poll);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }

            return View(poll);
        }

        // GET: Polls/Edit/5
        public ActionResult Edit(int? id)
        {
            string action = "Edit";

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Poll poll = db.Polls.Find(id);
            if (poll == null)
            {
                return HttpNotFound();
            }

            ViewBag.roles = GetRolesAllowed(action);

            return View(poll);
        }

        // POST: Polls/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "PollID,Title,NumQuestions,NumAnswers,questions,answers,For,Security")] Poll poll)
        {
            if (ModelState.IsValid)
            {
                poll.CreatedOn = db.Polls.Find(poll.PollID).CreatedOn;
                poll.Limit = db.Polls.Find(poll.PollID).Limit;

                db.Dispose();

                db = new SchoolDb();

                db.Entry(poll).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(poll);
        }

        // GET: Polls/Delete/5
        public ActionResult Delete(int? id)
        {
            string action = "Delete";

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Poll poll = db.Polls.Find(id);
            if (poll == null)
            {
                return HttpNotFound();
            }

            ViewBag.roles = GetRolesAllowed(action);

            return View(poll);
        }

        // POST: Polls/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Poll poll = db.Polls.Find(id);
            db.Polls.Remove(poll);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        //[AuthorizeRedirect(Roles="Administrator")]
        public ActionResult SearchStudent(string q)
        {
            string action = "SearchStudents";

            if (q != null)
            {
                List<ApplicationUser> users = context.Users
                    .Where(a => a.UserName.Contains(q))
                    .ToList();

                ViewBag.roles = GetRolesAllowed("StudentSearchSystem", action);

                return View(users);
            }
            else
            {
                var users = new List<ApplicationUser>();
               

                ViewBag.roles = GetRolesAllowed("StudentSearchSystem", action);
                return View(users);
            }

        }

        [HttpPost]
        //[AuthorizeRedirect(Roles = "Administrator")]
        public ActionResult SearchByYear( string q)
        {
            if (q != null)
            {
                List<ApplicationUser> users = context.Users
                    .Where(a => a.Year.Contains(q))
                    .ToList();

                ViewBag.roles = GetRolesAllowed("StudentSearchSystem", "SearchStudents");
                return View("SearchStudent", users);
            }
            else
            {
                var users = new List<ApplicationUser>();

                ViewBag.roles = GetRolesAllowed("StudentSearchSystem", "SearchStudents"); 
                return View("SearchStudent", users);
            }
        }

        [HttpPost]
      //[AuthorizeRedirect(Roles = "Administrator")]
        public ActionResult SearchByName(string q)
        {
            if (q != null)
            {
                List<ApplicationUser> users = context.Users
                    .Where(a => a.Name.Contains(q))
                    .ToList();

                ViewBag.roles = GetRolesAllowed("StudentSearchSystem", "SearchStudents");
                return View("SearchStudent", users);
            }
            else
            {
                var users = new List<ApplicationUser>();

                ViewBag.roles = GetRolesAllowed("StudentSearchSystem", "SearchStudents");
                return View("SearchStudent", users);
            }
        }

        [HttpPost]
        public ActionResult AwardHousePoints(List<string> users_list)
        {
            var a = users_list.ToArray();
            return RedirectToAction("Create", "HousePoints", new { users_list = users_list });
        }

        [HttpPost]
        //[AuthorizeRedirect(Roles = "Administrator")]
        public ActionResult SearchByTutorGroup(string q)
        {
            if (q != null)
            {
                List<ApplicationUser> users = context.Users
                    .Where(a => a.TutorGroup.Contains(q))
                    .ToList();

                ViewBag.roles = GetRolesAllowed("StudentSearchSystem", "SearchStudents");
                return View("SearchStudent", users);
            }
            else
            {
                var users = new List<ApplicationUser>();

                ViewBag.roles = GetRolesAllowed("StudentSearchSystem", "SearchStudents");
                return View("SearchStudent", users);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
