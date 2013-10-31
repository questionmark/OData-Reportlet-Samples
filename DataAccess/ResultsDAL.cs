using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using QM.Reporting.ODataDashboard.Web.Helpers;
using QM.Reporting.ODataDashboard.Web.Models;
using QM.Reporting.ODataDashboard.Web.QM.Analytics.OData;

namespace QM.Reporting.ODataDashboard.Web.DataAccess
{
    public class ResultsDAL
    {
        public static IEnumerable<Result> GetResultsForAssessment(AccountModel accountModel, int assessmentKey)
        {
            var results = new List<Result>();

            var context = DataAccessHelper.GetContext(accountModel);

            var query =
                context.Results
                    .Where(r => r.AssessmentKey == assessmentKey)
                    as DataServiceQuery;

            if (query != null)
            {
                var response = query.Execute() as QueryOperationResponse<Result>;
                if (response != null)
                {
                    results.AddRange(response);

                    var continuationToken = response.GetContinuation();
                    while (continuationToken != null)
                    {
                        response = context.Execute(continuationToken);
                        results.AddRange(response);

                        continuationToken = response.GetContinuation();
                    }
                }
            }

            return results;
        }
    }
}