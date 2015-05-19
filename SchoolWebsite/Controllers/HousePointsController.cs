using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SchoolWebsite.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace SchoolWebsite.Controllers
{
    public class HousePointsController : Controller
    {
        private SchoolDb db = new SchoolDb();

        // GET: HousePoints
        public ActionResult Index()
        {
            return View(db.HousePoints.ToList());
        }

        // GET: HousePoints/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HousePoint housePoint = db.HousePoints.Find(id);
            if (housePoint == null)
            {
                return HttpNotFound();
            }
            return View(housePoint);
        }

        // GET: HousePoints/Create
        public ActionResult Create(string users_list_str)
        {
            var um = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
    
            string initials = "";
            string awarding_teacher = um.FindByName(User.Identity.Name).Name;

            if(awarding_teacher.Contains("Mrs") ||
                awarding_teacher.Contains("Mr") ||
                awarding_teacher.Contains("Miss"))
            {
                initials += awarding_teacher.Substring(0, awarding_teacher.IndexOf(' ') + 1) + " ";
            }
            else
            {
                initials += awarding_teacher.Substring(0, 1);
                awarding_teacher = awarding_teacher.Remove(0, awarding_teacher.IndexOf(' ') + 1);
                initials += awarding_teacher.Substring(0, 1);
            }

            ViewBag.initials = initials;

            var array = users_list_str.Split(',');

            array = array.Where(item => item != "").ToArray();

            List<string> names = new List<string>();

            for(int i = 0; i < array.Count(); i++)
            {
                names.Add(um.FindByName(array[i]).Name);
            }

            ViewBag.users = array.ToList();
            ViewBag.names = names.ToArray();

            return View();
        }

        // POST: HousePoints/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "HousePointID,DateAwarded,AwardingTeacher,AwardedTo,Reason")] HousePoint housePoint)
        {
            if (ModelState.IsValid)
            {
                db.HousePoints.Add(housePoint);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(housePoint);
        }

        // GET: HousePoints/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HousePoint housePoint = db.HousePoints.Find(id);
            if (housePoint == null)
            {
                return HttpNotFound();
            }
            return View(housePoint);
        }

        // POST: HousePoints/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "HousePointID,DateAwarded,AwardingTeacher,Reason")] HousePoint housePoint)
        {
            if (ModelState.IsValid)
            {
                db.Entry(housePoint).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(housePoint);
        }

        // GET: HousePoints/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HousePoint housePoint = db.HousePoints.Find(id);
            if (housePoint == null)
            {
                return HttpNotFound();
            }
            return View(housePoint);
        }

        // POST: HousePoints/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            HousePoint housePoint = db.HousePoints.Find(id);
            db.HousePoints.Remove(housePoint);
            db.SaveChanges();
            return RedirectToAction("Index");
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
