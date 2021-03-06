﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using QM.Reporting.ODataDashboard.Web.Extensions;
using QM.Reporting.ODataDashboard.Web.QM.Analytics.OData;

namespace QM.Reporting.ODataDashboard.Web.Models
{
    public class ScoreDistributionModel
    {
        [DisplayName("Select Assessment")]
        public int SelectedAssessment { get; set; }

        public IEnumerable<SelectListItem> AssessmentItems { get; private set; }

        [DisplayName("Area")]
        public string TenantId { get; set; }

        [DisplayName("Username")]
        public string Username { get; set; }

        public ScoreDistributionModel(IEnumerable<Assessment> orderedAssessments, AccountModel accountModel)
        {
            SetAssessmentItems(orderedAssessments);

            TenantId = accountModel.TenantId;
            Username = accountModel.Username;
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