using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using SchoolWebsite.Models;
using System.Data.Entity;

namespace SchoolWebsite.Controllers
{
    //[AuthorizeRedirect(Roles="Administrator")]
    public class RolesController : Controller
    {
        public string SystemID = "RolesSystem";
        ApplicationDbContext context = new ApplicationDbContext();

        private SchoolDb db = new SchoolDb();

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


        //
        // GET: /Roles/
        public ActionResult Index()
        {
            
            var roles = context.Roles.ToList();

            ViewBag.roles = GetRolesAllowed("Delete");

            ModelState.AddModelError("", "This is all wrong!");

            return View(roles);
        }

        //
        // GET: /Roles/Create
        public ActionResult Create()
        {
            string action = "Create";
            ViewBag.roles = GetRolesAllowed(action);
            return View();
        }

        //
        // POST: /Roles/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                context.Roles.Add(new Microsoft.AspNet.Identity.EntityFramework.IdentityRole()
                {
                    Name = collection["RoleName"]
                });
                context.SaveChanges();
                ViewBag.ResultMessage = "Role created successfully !";
                ViewBag.roles = GetRolesAllowed("Create");
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        public void UpdateConfig(Config config, bool CheckValue, 
            string SystemID, string Action, string RoleName)
        {
            config = db.Configs.FirstOrDefault(a => a.SystemID == SystemID &&
                a.Action == Action);

            if (CheckValue == true)
            {
                if (config.RolesAllowed != null)
                {
                    if (config.RolesAllowed.Contains(RoleName))
                    {

                    }
                    else
                    {

                        if (config.RolesAllowed != "" && config.RolesAllowed != null)
                            config.RolesAllowed += ";" + RoleName;
                        else
                            config.RolesAllowed += RoleName;

                        db.Entry(config).State = EntityState.Modified;
                    }
                }
                else
                {
                    config.RolesAllowed += RoleName;
                    db.Entry(config).State = EntityState.Modified;
                }
                
            }
            else
            {
                
                if (config.RolesAllowed == null || !config.RolesAllowed.Contains(RoleName))
                {
                }
                else
                {
                    var list = config.RolesAllowed.Split(';').ToArray();
                    string roles_allowed = "";

                    for (int i = 0; i < list.Count(); i++)
                    {

                        if (list.GetValue(i).ToString() != RoleName)
                        {
                            if (roles_allowed != "" && roles_allowed != null)
                                roles_allowed += ";" + list.GetValue(i);
                            else
                                roles_allowed += list.GetValue(i);
                        }
                    }

                    config.RolesAllowed = roles_allowed;
                    db.Entry(config).State = EntityState.Modified;
                }
            }
        }

        [HttpPost]
        public ActionResult UpdatePowers(bool CreatePolls, bool EditPolls,
            bool DeletePolls, bool ViewAllPolls, bool SearchStudents, bool CreateRoles,
            bool EditRoles, bool DeleteRoles, bool ChangeRoles, bool CreateUsers,
            bool DeleteUsers, bool Create07, bool Create08, bool Create09,
            bool Create10, bool Create11, bool CreateAll, bool CreateTutorGroup,
            string RoleName)
        {
            Config config = new Config();

            db.Dispose();

            db = new SchoolDb();

            UpdateConfig(config, CreatePolls, "PollingSystem", "Create", RoleName);

            UpdateConfig(config, Create07, "PollingSystem", "Create07", RoleName);
            UpdateConfig(config, Create08, "PollingSystem", "Create08", RoleName);
            UpdateConfig(config, Create09, "PollingSystem", "Create09", RoleName);
            UpdateConfig(config, Create10, "PollingSystem", "Create10", RoleName);
            UpdateConfig(config, Create11, "PollingSystem", "Create11", RoleName);
            UpdateConfig(config, CreateAll, "PollingSystem", "CreateAll", RoleName);
            UpdateConfig(config, CreateTutorGroup, "PollingSystem", "CreateTutorGroup", RoleName);

            UpdateConfig(config, EditPolls, "PollingSystem", "Edit", RoleName);
            UpdateConfig(config, DeletePolls, "PollingSystem", "Delete", RoleName);
            UpdateConfig(config, ViewAllPolls, "PollingSystem", "ViewAllPolls", RoleName);

            UpdateConfig(config, SearchStudents, "StudentSearchSystem", "SearchStudents", RoleName);

            UpdateConfig(config, CreateRoles, "RolesSystem", "Create", RoleName);
            UpdateConfig(config, ChangeRoles, "RolesSystem", "ChangeRoles", RoleName);
            UpdateConfig(config, EditRoles, "RolesSystem", "Edit", RoleName);
            UpdateConfig(config, DeleteRoles, "RolesSystem", "Delete", RoleName);

            UpdateConfig(config, CreateUsers, "AccountsSystem", "Create", RoleName);
            UpdateConfig(config, DeleteUsers, "AccountsSystem", "Delete", RoleName);

            db.SaveChanges();

            //create config for creating and deleting users

            var roles = context.Roles.ToList();

            ViewBag.roles = GetRolesAllowed("Delete");

            return View("Index", roles);
        }

        //
        // GET: /Roles/Edit/5
        public ActionResult Edit(string roleName)
        {
            string action = "Edit";

            var thisRole = context.Roles.Where(r => r.Name.Equals(roleName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();

            ViewBag.roles = GetRolesAllowed(action);

            return View(thisRole);
        }

        //
        // POST: /Roles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Microsoft.AspNet.Identity.EntityFramework.IdentityRole role)
        {
            try
            {
                context.Entry(role).State = System.Data.Entity.EntityState.Modified;
                context.SaveChanges();

                ViewBag.roles = GetRolesAllowed("Delete");

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Roles/Delete/5
        public ActionResult Delete(string RoleName)
        {
            string action = "Delete";

            bool can_delete = false;

            List<string> roles = ViewBag.roles;

            foreach (string x in roles)
            {
                if (User.IsInRole(x))
                {
                    can_delete = true;
                }
            }

            if (can_delete)
            {
                var thisRole = context.Roles.Where(r => r.Name.Equals(RoleName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                context.Roles.Remove(thisRole);
                context.SaveChanges();
            }
            else
            {
                return Redirect("/Errors/Unauthorised");
            }

            ViewBag.roles = GetRolesAllowed("Delete");

            return RedirectToAction("Index");
        }

        public ActionResult ManageUserRoles(string q)
        {
            string action = "ChangeRoles";

            var list = context.Roles.OrderBy(r => r.Name).ToList().Select(rr => new SelectListItem { Value = rr.Name.ToString(), Text = rr.Name }).ToList();
            ViewBag.Roles = list;

            if(q == null)
            {

            }
            else
            {
                ViewBag.Username = q;
            }

            ViewBag.roles_allowed = GetRolesAllowed(action);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RoleAddToUser(string UserName, string RoleName)
        {
            ApplicationUser user = context.Users.Where(u => u.UserName.Equals(UserName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            var um = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
            var idResult = um.AddToRole(user.Id, RoleName);

            ViewBag.ResultMessage = "Role created successfully !";

            // prepopulat roles for the view dropdown
            var list = context.Roles.OrderBy(r => r.Name).ToList().Select(rr => new SelectListItem { Value = rr.Name.ToString(), Text = rr.Name }).ToList();
            ViewBag.Roles = list;

            ViewBag.roles_allowed = GetRolesAllowed("ChangeRoles");

            return View("ManageUserRoles");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GetRoles(string UserName)
        {
            if (!string.IsNullOrWhiteSpace(UserName))
            {
                ApplicationUser user = context.Users.Where(u => u.UserName.Equals(UserName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                var um = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
            
                ViewBag.RolesForThisUser = um.GetRoles(user.Id);

                // prepopulat roles for the view dropdown
                var list = context.Roles.OrderBy(r => r.Name).ToList().Select(rr => new SelectListItem { Value = rr.Name.ToString(), Text = rr.Name }).ToList();
                ViewBag.Roles = list;
            }

            ViewBag.roles_allowed = GetRolesAllowed("ChangeRoles");

            return View("ManageUserRoles");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteRoleForUser(string UserName, string RoleName)
        {
            var um = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
            ApplicationUser user = context.Users.Where(u => u.UserName.Equals(UserName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();

            if (um.IsInRole(user.Id, RoleName))
            {
                um.RemoveFromRole(user.Id, RoleName);
                ViewBag.ResultMessage = "Role removed from this user successfully !";
            }
            else
            {
                ViewBag.ResultMessage = "This user doesn't belong to selected role.";
            }
            // prepopulat roles for the view dropdown
            var list = context.Roles.OrderBy(r => r.Name).ToList().Select(rr => new SelectListItem { Value = rr.Name.ToString(), Text = rr.Name }).ToList();
            ViewBag.Roles = list;

            ViewBag.roles_allowed = GetRolesAllowed("ChangeRoles");

            return View("ManageUserRoles");
        }

        public ActionResult UnauthorisedError()
        {
            return View();
        }
    }
}