using System.Web.Mvc;
using System.Web.Security;
using QM.Reporting.ODataDashboard.Web.Helpers;
using QM.Reporting.ODataDashboard.Web.Models;

namespace QM.Reporting.ODataDashboard.Web.Controllers
{
    public class AccountController : Controller
    {
        //
        // GET: /Account/Login
        //
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Login(string returnUrl = "/")
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        //
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(AccountModel accountModel, string returnUrl)
        {
            ActionResult result;

            if (ModelState.IsValid && AuthHelper.LoginIsValid(accountModel))
            {
                AuthHelper.SaveAccountModelToFormsAuthCookie(Response, accountModel);

                // we don't use RedirectFromLoginPage() because it would overwrite the cookie
                result = new RedirectResult(returnUrl);
            }
            else
            {
                ViewBag.FailedLogin = true;
                ViewBag.ReturnUrl = returnUrl;
                result = View(accountModel);
            }

            return result;
        }

        // 
        // GET: /Account/Logout
        // 
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Logout(string returnUrl)
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", new { returnUrl });
        }
    }
}
