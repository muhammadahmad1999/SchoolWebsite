using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Data;
using System.Data.Entity;
using SchoolWebsite.Models;
using System.IO;
using System.Collections.Generic;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Web.Security;

namespace SchoolWebsite.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        public string SystemID = "AccountsSystem";

        ApplicationDbContext context = new ApplicationDbContext();

        private SchoolDb db = new SchoolDb();

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

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

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        [HttpGet]
        [Authorize()]
        public ActionResult Edit()
        {
            ApplicationUser user = context.Users.Find(User.Identity.GetUserId());
            if(user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Name,Year,TutorGroup,Email,BirthDate")] ApplicationUser user)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser original_user = context.Users.Find(User.Identity.GetUserId());

                original_user.Name = user.Name;
                original_user.Year = user.Year;
                original_user.TutorGroup = user.TutorGroup;
                original_user.Email = user.Email;
                original_user.BirthDate = user.BirthDate;

                context.Dispose();

                context = new ApplicationDbContext();

                context.Entry(original_user).State = EntityState.Modified;
                context.SaveChanges();
                return RedirectToLocal("/Manage");
            }
            return View(user);
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, shouldLockout: false);
                

            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        //[AuthorizeRedirect(Roles="Administrator")]
        public ActionResult Register()
        {
            ViewBag.roles = GetRolesAllowed("Create");

            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.UserName, Email = model.Email, BirthDate = model.BirthDate, Name = model.Name, Year = model.Year, TutorGroup = model.TutorGroup  };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent:false, rememberBrowser:false);
                    
                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }

            ViewBag.roles = GetRolesAllowed("Create");

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        public ActionResult RegisterMultiple()
        {
            ViewBag.error_msg = "";
            ViewBag.success_msg = "";

            ViewBag.roles = GetRolesAllowed("Create");

            var list = context.Roles.OrderBy(r => r.Name).ToList().Select(rr => new SelectListItem { Value = rr.Name.ToString(), Text = rr.Name }).ToList();
            ViewBag.Roles_available = list;

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> RegisterMultiple(HttpPostedFileBase file, string RoleName)
        {
            int num_user_created = 0, num_user_updated = 0, num_not_created = 0;

            List<RegisterViewModel> user_list = new List<RegisterViewModel>();

            if (file.ContentLength > 0)
            {
                //var fileName = Path.GetFileName(file.FileName);
                //var path = Path.Combine(Server.MapPath("~/App_Data/uploads"), fileName);
                //file.SaveAs(path);

                

                BinaryReader b = new BinaryReader(file.InputStream);
                byte[] binData = b.ReadBytes(file.ContentLength);

                string file_str = System.Text.Encoding.UTF8.GetString(binData);

                file_str = file_str.Replace("\r\n", "");

                string[] split_data = file_str.Split(',');

                List<string> file_list = split_data.ToList();

                file_list = file_list.Where(item => item != "").ToList();

                split_data = file_list.ToArray();
                
                string form_name = "", form_full_name = "";
                bool new_form = false;
                
                for (int i = 0; i < split_data.Count(); i++)
                {
                    //if(x == "" || x == null)
                    //{

                    //}

                    string x = split_data[i];

                    if (x.Contains("Male") && x.Contains("Female"))
                    {
                        new_form = false;
                    }

                    if (!new_form)
                    {
                        if (x.Contains("Registration"))
                        {
                            string y = x.Remove(0, x.IndexOf(":") + 2);

                            if (y.Contains("Mack"))
                            {
                                form_full_name = y.Substring(0, y.IndexOf(" "));
                                y = y.Remove(0, y.IndexOf(" ") + 1);

                                form_full_name += " " + y.Substring(0, y.IndexOf(" "));
                                y = y.Remove(0, y.IndexOf(" ") + 1);

                                string a = y.Substring(0, 1);

                                form_full_name += " " + a;
                                y = y.Remove(0, y.IndexOf(" ") + 1);

                                form_full_name += " " + y.Substring(0, y.IndexOf(" "));
                                a += y.Substring(0, 1);

                                form_name = "Mack" + " " + a;

                            }
                        }
                        else if (x.Contains("Reg Group"))
                        {
                            new_form = true;
                        }
                    }
                    else
                    {
                        RegisterViewModel model = new RegisterViewModel();

                        i = i - 1;

                        split_data[i] = split_data[i].Replace("Reg Group", "");
                        split_data[i] = split_data[i].Replace(form_full_name, "");

                        model.UserName = split_data[i];
                        model.Name = split_data[i + 2].Substring(1, split_data[i + 2].Length - 2) 
                            + " " + split_data[i + 1].Remove(0, 1);
                        model.BirthDate = Convert.ToDateTime(split_data[i + 4]);

                        string year = split_data[i + 5];
                        year = year.Replace("Year ", "");

                        if (year.Substring(0, 1) == "0")
                            model.Year = year.Remove(0, 1);
                        else
                            model.Year = year;

                        model.TutorGroup = form_name;

                        model.Email = model.UserName + "@prioryacademies.co.uk";
                        model.Password = "_Priory123";
                       
                        user_list.Add(model);

                        i = i + 6;
                    }

                }
            }

            
            //context.Dispose();
            //context = new ApplicationDbContext();

            var um = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));

            

            foreach (RegisterViewModel model in user_list)
            {
                var user = new ApplicationUser { UserName = model.UserName, Email = model.Email, BirthDate = model.BirthDate, Name = model.Name, Year = model.Year, TutorGroup = model.TutorGroup };
                var result = await um.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    //ApplicationUser user_a = context.Users.Where(u => u.UserName.Equals(model.UserName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();

                    ApplicationUser user_a = um.FindByName(model.UserName);

                    var idResult = um.AddToRole(user_a.Id, RoleName);

                    num_user_created++;
                }
                else if(um.FindByName(model.UserName) != null)
                {
                    ApplicationUser original_user = um.FindByName(user.UserName);

                    original_user.Name = user.Name;
                    original_user.Year = user.Year;
                    original_user.TutorGroup = user.TutorGroup;
                    original_user.Email = user.Email;
                    original_user.BirthDate = user.BirthDate;

                    um.Update(original_user);

                    num_user_updated++;
                }
                else
                {
                    num_not_created++;
                }
            }
            
            if (num_not_created > 0)
                ViewBag.error_msg = "Could not create " + num_not_created.ToString() + " users!";
            else
                ViewBag.error_msg = "";

            
            ViewBag.success_msg = "Successfully created " + num_user_created.ToString() +
                " users and updated " + num_user_updated + " users.";
                
            ViewBag.roles = GetRolesAllowed("Create");

            var list = context.Roles.OrderBy(r => r.Name).ToList().Select(rr => new SelectListItem { Value = rr.Name.ToString(), Text = rr.Name }).ToList();
            ViewBag.Roles_available = list;

            return View();
        }

        [HttpGet]
        public ActionResult DeleteMultiple()
        {
            ViewBag.error_msg = "";
            ViewBag.success_msg = "";

            ViewBag.roles = GetRolesAllowed("Delete");

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> DeleteMultiple(string user_str)
        {
            var um = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));

            int num_deleted = 0, num_not_deleted = 0;

            string[] split_data = user_str.Split(',');

            foreach (string x in split_data)
            {
                ApplicationUser user = um.FindByName(x);

                if (user != null)
                {
                    var results = await um.DeleteAsync(user);

                    // If successful
                    if (results.Succeeded)
                    {
                        num_deleted++;
                    }
                    else
                    {
                        num_not_deleted++;
                    }
                }
                else
                {
                    num_not_deleted++;
                }

            }

            if (num_not_deleted > 0)
                ViewBag.error_msg = "Could not delete " + num_not_deleted.ToString() + " users!";
            if(num_deleted > 0)
                ViewBag.success_msg = "Successfully deleted " + num_deleted.ToString() + " users!";

            ViewBag.roles = GetRolesAllowed("Delete");

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> DeleteMultipleFromExcel(HttpPostedFileBase file)
        {
            List<RegisterViewModel> user_list = new List<RegisterViewModel>();

            int num_deleted = 0, num_not_deleted = 0;

            if (file.ContentLength > 0)
            {
                //var fileName = Path.GetFileName(file.FileName);
                //var path = Path.Combine(Server.MapPath("~/App_Data/uploads"), fileName);
                //file.SaveAs(path);

                

                var um = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));

                BinaryReader b = new BinaryReader(file.InputStream);
                byte[] binData = b.ReadBytes(file.ContentLength);

                string file_str = System.Text.Encoding.UTF8.GetString(binData);

                file_str = file_str.Replace("\r\n", "");

                string[] split_data = file_str.Split(',');

                List<string> file_list = split_data.ToList();

                file_list = file_list.Where(item => item != "").ToList();

                split_data = file_list.ToArray();

                foreach (string x in file_list)
                {
                    ApplicationUser user = um.FindByName(x);

                    if (user != null)
                    {
                        var results = await um.DeleteAsync(user);
                        
                        // If successful
                        if (results.Succeeded)
                        {
                            num_deleted++;
                        }
                        else
                        {
                            num_not_deleted++;
                        }
                    }
                    else
                    {
                        num_not_deleted++;
                    }
                }
            }

            if(num_not_deleted > 0)
                ViewBag.error_msg = "Could not delete " + num_not_deleted.ToString() + " users!";
            if(num_deleted > 0)
                ViewBag.success_msg = "Successfully deleted " + num_deleted.ToString() + " users!";

            ViewBag.roles = GetRolesAllowed("Delete");

            return View("DeleteMultiple");
        }



        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}