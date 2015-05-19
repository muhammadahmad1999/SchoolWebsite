using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SchoolWebsite.Controllers
{
    public class ErrorsController : Controller
    {

        public ActionResult Unauthorised()
        {
            return View();
        }
    }
}