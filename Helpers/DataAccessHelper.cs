using System;
using System.Configuration;
using System.Net;
using QM.Reporting.ODataDashboard.Web.Models;
using QM.Reporting.ODataDashboard.Web.QM.Analytics.OData;

namespace QM.Reporting.ODataDashboard.Web.Helpers
{
    public class DataAccessHelper
    {
        /// <summary>
        /// Gets the odata context, using login details passed in to the method
        /// </summary>
        /// <param name="accountModel">The login details to use to connect</param>
        public static Entities GetContext(AccountModel accountModel)
        {
            var odataRoot = ConfigurationManager.AppSettings["odata.root"];
            var odataUrl = string.Format(odataRoot, accountModel.TenantId);
            var odataUri = new Uri(odataUrl);

            var context = new Entities(odataUri);

            var credentials = new NetworkCredential(accountModel.Username, accountModel.Password);
            context.Credentials = credentials;

            context.IgnoreResourceNotFoundException = true;

            return context;
        }
    }
}