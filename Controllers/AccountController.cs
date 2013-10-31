using System.Web.Mvc;
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
        public ActionResult Login(string returnUrl)
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
                ViewBag.ReturnUrl = returnUrl;
                result = View(accountModel);
            }

            return result;
        }
    }
}
