using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using QM.Reporting.ODataDashboard.Web.Enums;
using QM.Reporting.ODataDashboard.Web.Extensions;
using QM.Reporting.ODataDashboard.Web.QM.Analytics.OData;

namespace QM.Reporting.ODataDashboard.Web.Models
{
    public class PreTestPostTestModel
    {
        [DisplayName("First Assessment")]
        public int FirstAssessment { get; set; }

        [DisplayName("Attempt To Use")]
        public Attempt FirstAssessmentAttemptToUse { get; set; }

        [DisplayName("Attempt Number")]
        public int FirstAssessmentAttemptNumber { get; set; }

        [DisplayName("Second Assessment")]
        public int SecondAssessment { get; set; }

        [DisplayName("Attempt To Use")]
        public Attempt SecondAssessmentAttemptToUse { get; set; }

        [DisplayName("Attempt Number")]
        public int SecondAssessmentAttemptNumber { get; set; }

        public IEnumerable<SelectListItem> AssessmentItems { get; private set; }

        public PreTestPostTestModel(IEnumerable<Assessment> orderedAssessments)
        {
            SetAssessmentItems(orderedAssessments);
            FirstAssessmentAttemptToUse = Attempt.First; // default value
            FirstAssessmentAttemptNumber = 1; // default value
            SecondAssessmentAttemptToUse = Attempt.First; // default value
            SecondAssessmentAttemptNumber = 1; // default value
        }

        private void SetAssessmentItems(IEnumerable<Assessment> orderedAssessments)
        {
            var assessmentModels =
                orderedAssessments
                    .Select(a =>
                        new AssessmentModel(
                            a.AssessmentKey,
                            string.Format("{0} [ rev {1} ]", a.Name, a.RevisionNumber)));

            var assessmentModelList = assessmentModels.ToList();

            var assessmentsAsListItems =
                assessmentModelList.ToListItems(
                    a => a.AssessmentKey.ToString(),
                    a => a.AssessmentName);

            AssessmentItems = assessmentsAsListItems;
        }
    }
}