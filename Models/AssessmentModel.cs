namespace QM.Reporting.ODataDashboard.Web.Models
{
    public class AssessmentModel
    {
        public int AssessmentKey { get; set; }

        public string AssessmentName { get; set; }

        public AssessmentModel(int assessmentKey, string assessmentName)
        {
            AssessmentKey = assessmentKey;
            AssessmentName = assessmentName;
        }
    }
}