using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using QM.Reporting.ODataDashboard.Web.Helpers;
using QM.Reporting.ODataDashboard.Web.Models;
using QM.Reporting.ODataDashboard.Web.QM.Analytics.OData;

namespace QM.Reporting.ODataDashboard.Web.DataAccess
{
    public class AssessmentsDAL
    {
        /// <summary>
        /// Gets list of all assessments for the supplied login details
        /// </summary>
        /// <param name="accountModel">The login details</param>
        public static IEnumerable<Assessment> GetAssessments(AccountModel accountModel)
        {
            var assessments = new List<Assessment>();

            var context = DataAccessHelper.GetContext(accountModel);

            var query =
                context.Assessments
                    .OrderBy(a => a.Name)
                    .ThenBy(a => a.RevisionNumber)
                    as DataServiceQuery;

            if (query != null)
            {
                var response = query.Execute() as QueryOperationResponse<Assessment>;
                if (response != null)
                {
                    assessments.AddRange(response);

                    var continuationToken = response.GetContinuation();
                    while (continuationToken != null)
                    {
                        response = context.Execute(continuationToken);
                        assessments.AddRange(response);

                        continuationToken = response.GetContinuation();
                    }
                }
            }

            return assessments;
        }
    }
}