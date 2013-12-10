using System;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Security;
using QM.Reporting.ODataDashboard.Web.Models;

namespace QM.Reporting.ODataDashboard.Web.Helpers
{
    /// <summary>
    /// Contains methods to help with authentication and authorization
    /// </summary>
    public class AuthHelper
    {
        /// <summary>
        /// Checks whether the user-entered login details authenticate against the identity provider
        /// </summary>
        /// <param name="accountModel">user-entered login details</param>
        public static bool LoginIsValid(AccountModel accountModel)
        {
            var loginSuccessful = false;

            // Check if we have access by trying to access the odata feed for the specified tenant/user/password

            var context = DataAccessHelper.GetContext(accountModel);

            try
            {
                context.Assessments.Take(0).ToList();

                loginSuccessful = true;
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && (
                    ex.InnerException.Message.ToLower().Contains("401 - unauthorized") || 
                    ex.InnerException.Message.ToLower().Contains("401 unauthorized")))
                {
                    // login wasn't successful so swallow exception and prompt user again
                }
                else
                {
                    // something else went wrong so just dump out the exception
                    throw;
                }
            }

            return loginSuccessful;
        }

        /// <summary>
        /// Saves the user-entered login details into the forms authentication cookie
        /// </summary>
        /// <param name="response">The response to add the updated forms authentication cookie to</param>
        /// <param name="accountModel">The user-entered details to store in the cookie</param>
        public static void SaveAccountModelToFormsAuthCookie(HttpResponseBase response, AccountModel accountModel)
        {
            // serialize account model to a json string for storage
            var userData = Serialize(accountModel);

            // get the forms authentication cookie so we can update the forms authentication ticket stored within it
            var cookie = FormsAuthentication.GetAuthCookie(accountModel.Username, accountModel.RememberMe);

            // update the forms authentication ticket with our serialized account model
            var updatedTicket = GetUpdatedTicket(cookie, userData);

            // set the updated forms authentication ticket in the forms authentication cookie, encrypted of course
            cookie.Value = FormsAuthentication.Encrypt(updatedTicket);

            // add the forms authentication cookie back into the cookies collection
            response.Cookies.Add(cookie);
        }

        /// <summary>
        /// Gets the user-entered login details stored in the forms authentication cookie
        /// </summary>
        /// <param name="request">The request to read the forms authentication cookie from</param>
        public static AccountModel GetAccountModel(HttpRequestBase request)
        {
            var userData = GetUserData(request);

            var accountModel = Deserialize(userData);

            return accountModel;
        }

        private static string Serialize(AccountModel accountModel)
        {
            var jss = new JavaScriptSerializer();

            var userData = jss.Serialize(accountModel);

            return userData;
        }

        private static FormsAuthenticationTicket GetUpdatedTicket(HttpCookie cookie, string userData)
        {
            var oldTicket = FormsAuthentication.Decrypt(cookie.Value);

            if (oldTicket == null)
            {
                throw new Exception("Could not determine forms authentication cookie settings");
            }

            var updatedTicket = new FormsAuthenticationTicket(
                oldTicket.Version,
                oldTicket.Name,
                oldTicket.IssueDate,
                oldTicket.Expiration,
                oldTicket.IsPersistent,
                userData,
                oldTicket.CookiePath);

            return updatedTicket;
        }
        
        private static string GetUserData(HttpRequestBase request)
        {
            var cookie = request.Cookies[FormsAuthentication.FormsCookieName];

            if (cookie == null)
            {
                throw new Exception("Could not read forms authentication cookie");
            }

            var userData = string.Empty;

            var decryptedCookie = FormsAuthentication.Decrypt(cookie.Value);
            if (decryptedCookie != null)
            {
                userData = decryptedCookie.UserData ?? string.Empty;
            }

            return userData;
        }

        private static AccountModel Deserialize(string userData)
        {
            var jss = new JavaScriptSerializer();

            var accountModel = jss.Deserialize<AccountModel>(userData);

            return accountModel;
        }
    }
}