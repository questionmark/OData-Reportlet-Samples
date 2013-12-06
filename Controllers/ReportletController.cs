using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using QM.Reporting.ODataDashboard.Web.DataAccess;
using QM.Reporting.ODataDashboard.Web.Enums;
using QM.Reporting.ODataDashboard.Web.Helpers;
using QM.Reporting.ODataDashboard.Web.Models;
using QM.Reporting.ODataDashboard.Web.QM.Analytics.OData;

namespace QM.Reporting.ODataDashboard.Web.Controllers
{
    public class ReportletController : Controller
    {
        #region * Attempt Distribution *

        //
        // GET /Reportlet/AttemptDistribution
        //
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult AttemptDistribution()
        {
            var accountModel = AuthHelper.GetAccountModel(Request);

            var assessments = AssessmentsDAL.GetAssessments(accountModel);

            var attemptDistributionModel = new AttemptDistributionModel(assessments);

            return View(attemptDistributionModel);
        }

        //
        // GET: /Reportlet/AttemptDistributionData/
        //
        [AcceptVerbs(HttpVerbs.Get)]
        public string AttemptDistributionData(int selectedAssessment = 0, string scorebandNameForPass = "Pass")
        {
            var accountModel = AuthHelper.GetAccountModel(Request);

            var results = ResultsDAL.GetResultsForAssessment(accountModel, selectedAssessment).ToList();

            var firstPassedResults = GetFirstPassedResults(scorebandNameForPass, results);

            var totalAttempts = firstPassedResults.Count;
            var meanAttempts = GetRoundedMeanAttempts(firstPassedResults);
            var standardDeviationAttempts = GetRoundedStandardDeviationAttempts(firstPassedResults, meanAttempts);
            var medianAttempts = GetRoundedMedianAttempts(firstPassedResults);
            var highestResultCount = GetHighestResultCount(firstPassedResults);
            var chartData = GetAttemptDistributionChartData(firstPassedResults);

            var meanAttemptsAsString =
                !double.IsNaN(meanAttempts)
                    ? string.Format("{0:F1}", meanAttempts)
                    : "-";
            var standardDeviationAttemptsAsString =
                !double.IsNaN(standardDeviationAttempts)
                    ? string.Format("{0:F1}", standardDeviationAttempts)
                    : "-";
            var medianAttemptsAsString =
                !double.IsNaN(medianAttempts)
                    ? string.Format("{0:F1}", medianAttempts)
                    : "-";

            var jsonData =
                string.Format(@"
                    {{
                        ""totalAttempts"":[{0}],
                        ""meanAttempts"":[""{1}""],
                        ""standardDeviationAttempts"":[""{2}""],
                        ""medianAttempts"":[""{3}""],
                        ""highestResultCount"":[{4}],
                        ""chartData"":[{5}]
                    }}",
                    totalAttempts,
                    meanAttemptsAsString,
                    standardDeviationAttemptsAsString,
                    medianAttemptsAsString,
                    highestResultCount,
                    chartData);

            return jsonData;
        }

        private static List<Result> GetFirstPassedResults(string scorebandNameForPass, List<Result> results)
        {
            // only count attempts up to the first Pass result
            // so lets remove all of the attempts after the first Pass result for each participant

            var firstPassedResults = new List<Result>();

            var participantKeys =
                results
                    .Select(r => r.ParticipantKey)
                    .Distinct();

            foreach (var participantKey in participantKeys)
            {
                // get all results for this participant
                var participantResults =
                    results
                        .Where(r => r.ParticipantKey == participantKey)
                        .ToList();

                // get all results for this participant that were Passes
                var passedAttempts =
                    participantResults
                        .Where(pr => pr.ScorebandName.Equals(scorebandNameForPass, StringComparison.InvariantCultureIgnoreCase))
                        .ToList();

                // determine the first attempt that was a Pass (lowest attempt number)
                // if no Passed attempts found, just select the highest attempt number
                var firstPassedAttemptNumber =
                    passedAttempts.Any()
                        ? passedAttempts.Min(pa => pa.AssessmentAttemptNumber)
                        : participantResults.Max(pr => pr.AssessmentAttemptNumber);

                // add only the results up to the first attempt that was a Pass
                // if no Passed attempts were found, this will just add all attempts
                firstPassedResults.AddRange(
                    participantResults
                        .Where(r => r.AssessmentAttemptNumber <= firstPassedAttemptNumber));
            }

            return firstPassedResults;
        }

        private static double GetRoundedMeanAttempts(List<Result> results)
        {
            // if there are no results, mean attempts can't be calculated
            if (results.Count == 0)
                return double.NaN;

            var numberOfResults = results.Count;
            var totalAttempts = 0.0;

            foreach (var result in results)
            {
                totalAttempts += result.AssessmentAttemptNumber;
            }
            var meanAttempts = totalAttempts / numberOfResults;

            var roundedMeanAttempts = Math.Round(meanAttempts, 1, MidpointRounding.AwayFromZero);

            return roundedMeanAttempts;
        }

        private static double GetRoundedStandardDeviationAttempts(List<Result> results, double meanAttempts)
        {
            // if there are no results, standard deviation can't be calculated
            if (results.Count == 0)
                return double.NaN;

            // if there is only one result, standard deviation is zero
            if (results.Count == 1)
                return 0;

            // there is more than one result, so lets calculate the standard deviation
            var numberOfResultsAsDouble = (double)results.Count;
            var deviationSquaredSum = 0.0;

            foreach (var result in results)
            {
                var deviation = result.AssessmentAttemptNumber - meanAttempts;
                var deviationSquared = deviation * deviation;
                deviationSquaredSum += deviationSquared;
            }
            var deviationSquaredMean = deviationSquaredSum * (1 / (numberOfResultsAsDouble - 1));
            var standardDeviation = Math.Sqrt(deviationSquaredMean);

            var roundedStandardDeviation = Math.Round(standardDeviation, 1, MidpointRounding.AwayFromZero);

            return roundedStandardDeviation;
        }

        private static double GetRoundedMedianAttempts(List<Result> results)
        {
            // if there are no results, mean attempts can't be calculated
            if (results.Count == 0)
                return double.NaN;

            var orderedResults = results.OrderBy(r => r.AssessmentAttemptNumber).ToList();

            var numberOfResults = orderedResults.Count;
            var medianAttempts = 0.0;

            if (numberOfResults % 2 == 0)
            {
                // even number of results
                // find the middle two attempt numbers and average them
                var firstMiddle = ( numberOfResults / 2 ) - 1;
                var secondMiddle = firstMiddle + 1;

                var firstMiddleAttemptNumber = orderedResults[firstMiddle].AssessmentAttemptNumber;
                var secondMiddleAttemptNumber = orderedResults[secondMiddle].AssessmentAttemptNumber;
                
                medianAttempts = ( firstMiddleAttemptNumber + secondMiddleAttemptNumber ) / 2.0;
            }
            else
            {
                // odd number of results
                // find the middle attempt number
                var middle = numberOfResults / 2;

                medianAttempts = orderedResults[middle].AssessmentAttemptNumber;
            }

            var roundedMedianAttempts = Math.Round(medianAttempts, 1, MidpointRounding.AwayFromZero);

            return roundedMedianAttempts;
        }

        private static string GetAttemptDistributionChartData(List<Result> results)
        {
            var chartData = string.Empty;

            var maxAttemptNumber = GetMaxAttemptNumber(results);

            for (int attemptNumber = 1; attemptNumber <= maxAttemptNumber; attemptNumber++)
            {
                var count = results.Count(r => r.AssessmentAttemptNumber == attemptNumber);

                chartData += string.Format("[\"Attempt {0}\",{1}],", attemptNumber, count);
            }
            chartData = chartData.TrimEnd(',');

            return chartData;
        }

        #endregion

        #region * PreTest PostTest *

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult PreTestPostTest()
        {
            var accountModel = AuthHelper.GetAccountModel(Request);

            var assessments = AssessmentsDAL.GetAssessments(accountModel);

            var preTestPostTestModel = new PreTestPostTestModel(assessments);

            return View(preTestPostTestModel);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public string PreTestPostTestData(
            int firstAssessment = 0,
            Attempt firstAssessmentAttemptToUse = Attempt.First,
            int firstAssessmentAttemptNumber = 0,
            int secondAssessment = 0,
            Attempt secondAssessmentAttemptToUse = Attempt.First,
            int secondAssessmentAttemptNumber = 0)
        {
            var accountModel = AuthHelper.GetAccountModel(Request);

            var resultsForFirstAssessment = ResultsDAL.GetResultsForAssessment(accountModel, firstAssessment).ToList();
            var resultsForSecondAssessment = ResultsDAL.GetResultsForAssessment(accountModel, secondAssessment).ToList();

            var singleAttemptResultsForFirstAssessment = FilterResultsForAttemptNumber(resultsForFirstAssessment, firstAssessmentAttemptToUse, firstAssessmentAttemptNumber);
            var singleAttemptResultsForSecondAssessment = FilterResultsForAttemptNumber(resultsForSecondAssessment, secondAssessmentAttemptToUse, secondAssessmentAttemptNumber);

            var filteredResultsForFirstAssessment = FilterResultsMissingFromOtherAssessment(singleAttemptResultsForFirstAssessment, singleAttemptResultsForSecondAssessment);
            var filteredResultsForSecondAssessment = FilterResultsMissingFromOtherAssessment(singleAttemptResultsForSecondAssessment, singleAttemptResultsForFirstAssessment);

            var countOfFilteredResultsForFirstAssessment = filteredResultsForFirstAssessment.Count;
            var countOfIgnoredResultsForFirstAssessment = singleAttemptResultsForFirstAssessment.Count - filteredResultsForFirstAssessment.Count;
            var meanScoreOfFilteredResultsForFirstAssessment = GetRoundedMeanPercentageScore(filteredResultsForFirstAssessment);
            var standardDeviationOfFilteredResultsForFirstAssessment = GetRoundedStandardDeviation(filteredResultsForFirstAssessment, meanScoreOfFilteredResultsForFirstAssessment);
            var medianScoreOfFilteredResultsForFirstAssessment = GetRoundedMedianPercentageScore(filteredResultsForFirstAssessment);

            var meanScoreOfFilteredResultsForFirstAssessmentAsString =
                !double.IsNaN(meanScoreOfFilteredResultsForFirstAssessment)
                    ? string.Format("{0:F1}%", meanScoreOfFilteredResultsForFirstAssessment)
                    : "-";
            var standardDeviationOfFilteredResultsForFirstAssessmentAsString =
                !double.IsNaN(standardDeviationOfFilteredResultsForFirstAssessment)
                    ? string.Format("{0:F1}%", standardDeviationOfFilteredResultsForFirstAssessment)
                    : "-";
            var medianScoreOfFilteredResultsForFirstAssessmentAsString =
                !double.IsNaN(medianScoreOfFilteredResultsForFirstAssessment)
                    ? string.Format("{0:F1}%", medianScoreOfFilteredResultsForFirstAssessment)
                    : "-";

            var countOfFilteredResultsForSecondAssessment = filteredResultsForSecondAssessment.Count;
            var countOfIgnoredResultsForSecondAssessment = singleAttemptResultsForSecondAssessment.Count - filteredResultsForSecondAssessment.Count;
            var meanScoreOfFilteredResultsForSecondAssessment = GetRoundedMeanPercentageScore(filteredResultsForSecondAssessment);
            var standardDeviationOfFilteredResultsForSecondAssessment = GetRoundedStandardDeviation(filteredResultsForSecondAssessment, meanScoreOfFilteredResultsForSecondAssessment);
            var medianScoreOfFilteredResultsForSecondAssessment = GetRoundedMedianPercentageScore(filteredResultsForSecondAssessment);

            var meanScoreOfFilteredResultsForSecondAssessmentAsString =
                !double.IsNaN(meanScoreOfFilteredResultsForSecondAssessment)
                    ? string.Format("{0:F1}%", meanScoreOfFilteredResultsForSecondAssessment)
                    : "-";
            var standardDeviationOfFilteredResultsForSecondAssessmentAsString =
                !double.IsNaN(standardDeviationOfFilteredResultsForSecondAssessment)
                    ? string.Format("{0:F1}%", standardDeviationOfFilteredResultsForSecondAssessment)
                    : "-";
            var medianScoreOfFilteredResultsForSecondAssessmentAsString =
                !double.IsNaN(medianScoreOfFilteredResultsForSecondAssessment)
                    ? string.Format("{0:F1}%", medianScoreOfFilteredResultsForSecondAssessment)
                    : "-";

            var quartileScoresForFirstAssessment = GetQuartileScores(filteredResultsForFirstAssessment);
            var quartileScoresForSecondAssessment = GetQuartileScores(filteredResultsForSecondAssessment);

            var q0ScoreForFirstAssessmentAsString =
                !double.IsNaN(quartileScoresForFirstAssessment.Quartile0Score)
                    ? string.Format("{0:F1}%", quartileScoresForFirstAssessment.Quartile0Score)
                    : "-";
            var q1ScoreForFirstAssessmentAsString =
                !double.IsNaN(quartileScoresForFirstAssessment.Quartile1Score)
                    ? string.Format("{0:F1}%", quartileScoresForFirstAssessment.Quartile1Score)
                    : "-";
            var q2ScoreForFirstAssessmentAsString = 
                !double.IsNaN(quartileScoresForFirstAssessment.Quartile2Score)
                    ? string.Format("{0:F1}%", quartileScoresForFirstAssessment.Quartile2Score)
                    : "-";
            var q3ScoreForFirstAssessmentAsString = 
                !double.IsNaN(quartileScoresForFirstAssessment.Quartile3Score)
                    ? string.Format("{0:F1}%", quartileScoresForFirstAssessment.Quartile3Score)
                    : "-";
            var q4ScoreForFirstAssessmentAsString = 
                !double.IsNaN(quartileScoresForFirstAssessment.Quartile4Score)
                    ? string.Format("{0:F1}%", quartileScoresForFirstAssessment.Quartile4Score)
                    : "-";
            var q0ScoreForSecondAssessmentAsString = 
                !double.IsNaN(quartileScoresForSecondAssessment.Quartile0Score)
                    ? string.Format("{0:F1}%", quartileScoresForSecondAssessment.Quartile0Score)
                    : "-";
            var q1ScoreForSecondAssessmentAsString = 
                !double.IsNaN(quartileScoresForSecondAssessment.Quartile1Score)
                    ? string.Format("{0:F1}%", quartileScoresForSecondAssessment.Quartile1Score)
                    : "-";
            var q2ScoreForSecondAssessmentAsString = 
                !double.IsNaN(quartileScoresForSecondAssessment.Quartile2Score)
                    ? string.Format("{0:F1}%", quartileScoresForSecondAssessment.Quartile2Score)
                    : "-";
            var q3ScoreForSecondAssessmentAsString = 
                !double.IsNaN(quartileScoresForSecondAssessment.Quartile3Score)
                    ? string.Format("{0:F1}%", quartileScoresForSecondAssessment.Quartile3Score)
                    : "-";
            var q4ScoreForSecondAssessmentAsString = 
                !double.IsNaN(quartileScoresForSecondAssessment.Quartile4Score)
                    ? string.Format("{0:F1}%", quartileScoresForSecondAssessment.Quartile4Score)
                    : "-";

            var nameForFirstAssessment = resultsForFirstAssessment.Count > 0 ? resultsForFirstAssessment[0].AssessmentName : "-";
            var nameForSecondAssessment = resultsForSecondAssessment.Count > 0 ? resultsForSecondAssessment[0].AssessmentName : "-";

            var chartData = GetPreTestPostTestChartData(nameForFirstAssessment, quartileScoresForFirstAssessment, nameForSecondAssessment, quartileScoresForSecondAssessment);

            var jsonData = string.Format(@"
                {{
                    ""countOfFilteredResultsForFirstAssessment"":{0},
                    ""countOfIgnoredResultsForFirstAssessment"":{1},
                    ""meanScoreOfFilteredResultsForFirstAssessment"":""{2}"",
                    ""standardDeviationOfFilteredResultsForFirstAssessment"":""{3}"",
                    ""medianScoreOfFilteredResultsForFirstAssessment"":""{4}"",
                    ""quartile0ScoreForFirstAssessment"":""{5}"",
                    ""quartile1ScoreForFirstAssessment"":""{6}"",
                    ""quartile2ScoreForFirstAssessment"":""{7}"",
                    ""quartile3ScoreForFirstAssessment"":""{8}"",
                    ""quartile4ScoreForFirstAssessment"":""{9}"",
                    ""countOfFilteredResultsForSecondAssessment"":{10},
                    ""countOfIgnoredResultsForSecondAssessment"":{11},
                    ""meanScoreOfFilteredResultsForSecondAssessment"":""{12}"",
                    ""standardDeviationOfFilteredResultsForSecondAssessment"":""{13}"",
                    ""medianScoreOfFilteredResultsForSecondAssessment"":""{14}"",
                    ""quartile0ScoreForSecondAssessment"":""{15}"",
                    ""quartile1ScoreForSecondAssessment"":""{16}"",
                    ""quartile2ScoreForSecondAssessment"":""{17}"",
                    ""quartile3ScoreForSecondAssessment"":""{18}"",
                    ""quartile4ScoreForSecondAssessment"":""{19}"",
                    ""chartData"":[{20}]
                }}",
                countOfFilteredResultsForFirstAssessment,
                countOfIgnoredResultsForFirstAssessment,
                meanScoreOfFilteredResultsForFirstAssessmentAsString,
                standardDeviationOfFilteredResultsForFirstAssessmentAsString,
                medianScoreOfFilteredResultsForFirstAssessmentAsString,
                q0ScoreForFirstAssessmentAsString,
                q1ScoreForFirstAssessmentAsString,
                q2ScoreForFirstAssessmentAsString,
                q3ScoreForFirstAssessmentAsString,
                q4ScoreForFirstAssessmentAsString,
                countOfFilteredResultsForSecondAssessment,
                countOfIgnoredResultsForSecondAssessment,
                meanScoreOfFilteredResultsForSecondAssessmentAsString,
                standardDeviationOfFilteredResultsForSecondAssessmentAsString,
                medianScoreOfFilteredResultsForSecondAssessmentAsString,
                q0ScoreForSecondAssessmentAsString,
                q1ScoreForSecondAssessmentAsString,
                q2ScoreForSecondAssessmentAsString,
                q3ScoreForSecondAssessmentAsString,
                q4ScoreForSecondAssessmentAsString,
                chartData);

            return jsonData;
        }

        private static List<Result> FilterResultsForAttemptNumber(List<Result> results, Attempt attemptToUse, int attemptNumber)
        {
            var filteredResults = new List<Result>();

            if (attemptToUse == Attempt.First)
            {
                filteredResults = results.Where(r => r.AssessmentAttemptNumber == 1).ToList();
            }
            else if (attemptToUse == Attempt.Number)
            {
                filteredResults = results.Where(r => r.AssessmentAttemptNumber == attemptNumber).ToList();
            }
            else
            {
                filteredResults = results.OrderBy(r => r.ParticipantKey).ThenBy(r => r.AssessmentAttemptNumber).ToList();
                var participantKey = 0;
                for (int i = filteredResults.Count - 1; i >= 0; i--)
                {
                    if (filteredResults[i].ParticipantKey == participantKey)
                    {
                        filteredResults.RemoveAt(i);
                    }
                    else
                    {
                        participantKey = filteredResults[i].ParticipantKey;
                    }
                }
            }

            return filteredResults;
        }

        private static List<Result> FilterResultsMissingFromOtherAssessment(List<Result> resultsToFilter, List<Result> resultsToCheckAgainst)
        {
            var filteredResults = new List<Result>();

            foreach (var result in resultsToFilter)
            {
                if (resultsToCheckAgainst.Any(r => r.ParticipantKey == result.ParticipantKey))
                {
                    filteredResults.Add(result);
                }
            }

            return filteredResults;
        }

        private static QuartileScores GetQuartileScores(List<Result> results)
        {
            var quartileScores = new QuartileScores();

            if (results.Count > 0)
            {
                var orderedResults = results.OrderBy(r => r.ResultPercentageScore).ToList();

                var lowRangeStart = 0;
                var lowRangeEnd = orderedResults.Count / 2;
                var highRangeStart = orderedResults.Count / 2;
                var highRangeEnd = orderedResults.Count - 1;

                if (orderedResults.Count % 2 == 0)
                {
                    // even number of results so slightly adjust the ranges
                    lowRangeEnd -= 1;
                }

                var lowRange = new List<Result>();
                for (int i = lowRangeStart; i <= lowRangeEnd; i++)
                {
                    lowRange.Add(orderedResults[i]);
                }
                var highRange = new List<Result>();
                for (int i = highRangeStart; i <= highRangeEnd; i++)
                {
                    highRange.Add(orderedResults[i]);
                }

                quartileScores.Quartile0Score = orderedResults[lowRangeStart].ResultPercentageScore;
                quartileScores.Quartile1Score = GetMedianScore(lowRange);
                quartileScores.Quartile2Score = GetMedianScore(orderedResults);
                quartileScores.Quartile3Score = GetMedianScore(highRange);
                quartileScores.Quartile4Score = orderedResults[highRangeEnd].ResultPercentageScore;
            }

            return quartileScores;
        }

        private static string GetPreTestPostTestChartData(string nameForFirstAssessment, QuartileScores quartileScoresForFirstAssessment, string nameForSecondAssessment, QuartileScores quartileScoresForSecondAssessment)
        {
            var chartData = string.Empty;

            chartData += GetPreTestPostTestChartData(nameForFirstAssessment, quartileScoresForFirstAssessment);
            chartData += GetPreTestPostTestChartData(nameForSecondAssessment, quartileScoresForSecondAssessment);

            chartData = chartData.TrimEnd(',');

            return chartData;
        }

        private static string GetPreTestPostTestChartData(string assessmentName, QuartileScores quartileScores)
        {
            var tooltip = string.Format(
                "Quartile 4: {0}<br/>" +
                "Quartile 3: {1}<br/>" +
                "Quartile 2: {2}<br/>" +
                "Quartile 1: {3}<br/>" +
                "Quartile 0: {4}",
                !double.IsNaN(quartileScores.Quartile4Score) ? string.Format("{0:F1}%", quartileScores.Quartile4Score) : "-",
                !double.IsNaN(quartileScores.Quartile3Score) ? string.Format("{0:F1}%", quartileScores.Quartile3Score) : "-",
                !double.IsNaN(quartileScores.Quartile2Score) ? string.Format("{0:F1}%", quartileScores.Quartile2Score) : "-",
                !double.IsNaN(quartileScores.Quartile1Score) ? string.Format("{0:F1}%", quartileScores.Quartile1Score) : "-",
                !double.IsNaN(quartileScores.Quartile0Score) ? string.Format("{0:F1}%", quartileScores.Quartile0Score) : "-"
                );

            var chartData = string.Format(@"[""{0}"",{1},{2},{3},{4},""{5}""],",
                assessmentName,
                !double.IsNaN(quartileScores.Quartile0Score) ? Math.Round(quartileScores.Quartile0Score, 1, MidpointRounding.AwayFromZero) : 0,
                !double.IsNaN(quartileScores.Quartile1Score) ? Math.Round(quartileScores.Quartile1Score, 1, MidpointRounding.AwayFromZero) : 0,
                !double.IsNaN(quartileScores.Quartile3Score) ? Math.Round(quartileScores.Quartile3Score, 1, MidpointRounding.AwayFromZero) : 0,
                !double.IsNaN(quartileScores.Quartile4Score) ? Math.Round(quartileScores.Quartile4Score, 1, MidpointRounding.AwayFromZero) : 0,
                tooltip
                );

            return chartData;
        }

        private static double GetMedianScore(List<Result> results)
        {
            double medianScore;

            if (results.Count % 2 == 0)
            {
                // even number of results, so average the middle two
                var midPos1 = (results.Count / 2) - 1;
                var midPos2 = results.Count / 2;
                var midScore1 = results[midPos1].ResultPercentageScore;
                var midScore2 = results[midPos2].ResultPercentageScore;
                medianScore = ( midScore1 + midScore2 ) / 2;
            }
            else
            {
                // odd number of results, so pick the middle one
                var midPos = results.Count / 2;
                medianScore = results[midPos].ResultPercentageScore;
            }

            return medianScore;
        }

        #endregion

        #region * Score Correlation *

        //
        // GET /Reportlet/ScoreCorrelation
        //
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult ScoreCorrelation()
        {
            var accountModel = AuthHelper.GetAccountModel(Request);

            var assessments = AssessmentsDAL.GetAssessments(accountModel);

            var scoreCorrelationModel = new ScoreCorrelationModel(assessments);

            return View(scoreCorrelationModel);
        }

        //
        // GET /Reportlet/ScoreCorrelationData
        //
        [AcceptVerbs(HttpVerbs.Get)]
        public string ScoreCorrelationData(int firstSelectedAssessment = 0, int secondSelectedAssessment = 0)
        {
            var accountModel = AuthHelper.GetAccountModel(Request);

            var resultsForFirstAssessment = ResultsDAL.GetResultsForAssessment(accountModel, firstSelectedAssessment).ToList();
            var resultsForSecondAssessment = ResultsDAL.GetResultsForAssessment(accountModel, secondSelectedAssessment).ToList();

            // only look at the first attempts for each result to ensure a valid comparison
            var resultsForFirstAssessmentFirstAttempt = GetOnlyFirstAttempts(resultsForFirstAssessment);
            var resultsForSecondAssessmentFirstAttempt = GetOnlyFirstAttempts(resultsForSecondAssessment);

            var chartData = GetScoreCorrelationChartData(resultsForFirstAssessmentFirstAttempt, resultsForSecondAssessmentFirstAttempt);

            var jsonData = string.Format(@"
                {{
                    ""chartData"":[{0}]
                }}",
                chartData);

            return jsonData;
        }

        private static List<Result> GetOnlyFirstAttempts(List<Result> results)
        {
            var resultsForFirstAttempts = results.Where(r => r.AssessmentAttemptNumber == 1).ToList();

            return resultsForFirstAttempts;
        }

        private static string GetScoreCorrelationChartData(List<Result> resultsForFirstAssessment, List<Result> resultsForSecondAssessment)
        {
            var chartData = string.Empty;

            foreach (var firstAssessmentResult in resultsForFirstAssessment)
            {
                var secondAssessmentResult =
                    resultsForSecondAssessment
                        .FirstOrDefault(r => r.ParticipantKey == firstAssessmentResult.ParticipantKey);

                // we only return scores for participants that have a result in both the first and second assessment
                if (secondAssessmentResult != null)
                {
                    var firstAssessmentScore = Math.Round(firstAssessmentResult.ResultPercentageScore, 1, MidpointRounding.AwayFromZero);
                    var secondAssessmentScore = Math.Round(secondAssessmentResult.ResultPercentageScore, 1, MidpointRounding.AwayFromZero);

                    chartData += string.Format("[{0},{1},\"{0}%, {1}%\"],", firstAssessmentScore, secondAssessmentScore);
                }
            }
            chartData = chartData.TrimEnd(',');

            return chartData;
        }

        #endregion

        #region * Score Distribution *

        //
        // GET: /Reportlet/ScoreDistribution/
        //
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult ScoreDistribution()
        {
            var accountModel = AuthHelper.GetAccountModel(Request);

            var assessments = AssessmentsDAL.GetAssessments(accountModel);

            var scoreDistributionModel = new ScoreDistributionModel(assessments);

            return View(scoreDistributionModel);
        }

        //
        // GET: /Reportlet/ScoreDistributionData/
        //
        [AcceptVerbs(HttpVerbs.Get)]
        public string ScoreDistributionData(int selectedAssessment = 0)
        {
            var accountModel = AuthHelper.GetAccountModel(Request);

            var results = ResultsDAL.GetResultsForAssessment(accountModel, selectedAssessment).ToList();

            var highestResultCount = GetHighestResultCountGroupedByPercentile(results);
            var numberOfResults = results.Count;
            var meanPercentageScore = GetRoundedMeanPercentageScore(results);
            var standardDeviation = GetRoundedStandardDeviation(results, meanPercentageScore);
            var chartData = GetScoreDistributionChartData(results);

            var meanPercentageScoreAsString =
                !double.IsNaN(meanPercentageScore)
                    ? string.Format("{0}%", meanPercentageScore)
                    : "-";
            var standardDeviationAsString =
                !double.IsNaN(standardDeviation)
                    ? string.Format("{0}%", standardDeviation)
                    : "-";

            var jsonData =
                string.Format(@"
                    {{
                        ""highestResultCount"":{0},
                        ""numberOfResults"":{1},
                        ""meanPercentageScore"":""{2}"",
                        ""standardDeviation"":""{3}"",
                        ""chartData"":[{4}]
                    }}",
                    highestResultCount,
                    numberOfResults,
                    meanPercentageScoreAsString,
                    standardDeviationAsString,
                    chartData);

            return jsonData;
        }

        private static string GetScoreDistributionChartData(List<Result> results)
        {
            var chartData = string.Empty;

            for (int rangeMaxScore = 10; rangeMaxScore <= 100; rangeMaxScore += 10)
            {
                var count =
                    results
                        .Count(r => r.ResultPercentageScore > rangeMaxScore - 10 &&
                                    r.ResultPercentageScore <= rangeMaxScore);

                if (rangeMaxScore == 10)
                {
                    // This is the 0%-10% group.  If you look at the code above, you'll see it actually
                    // selects greater than 0%, so any results with exactly 0% are not included.  So we 
                    // include them specifically here.
                    count +=
                        results
                            .Count(r => r.ResultPercentageScore.Equals(0.0));
                }

                chartData += string.Format("[\"{0}-{1}%\",{2}],", rangeMaxScore - 10, rangeMaxScore, count);
            }
            chartData = chartData.TrimEnd(',');

            return chartData;
        }

        private static int GetHighestResultCountGroupedByPercentile(List<Result> results)
        {
            var highestCount = 0;

            for (int rangeMaxScore = 10; rangeMaxScore <= 100; rangeMaxScore += 10)
            {
                var countForRange =
                    results
                        .Count(r => r.ResultPercentageScore > rangeMaxScore - 10 &&
                                    r.ResultPercentageScore <= rangeMaxScore);

                if (countForRange > highestCount)
                {
                    highestCount = countForRange;
                }
            }

            return highestCount;
        }

        #endregion

        private static int GetHighestResultCount(List<Result> results)
        {
            var highestResultCount =
                results.Count > 0
                    ? results.GroupBy(r => r.AssessmentAttemptNumber).Select(rg => rg.Count()).Max()
                    : 0;

            return highestResultCount;
        }

        private static int GetMaxAttemptNumber(List<Result> results)
        {
            var maxAttemptNumber =
                results.Count > 0
                    ? results.Max(r => r.AssessmentAttemptNumber)
                    : 0;

            return maxAttemptNumber;
        }

        private static double GetRoundedMeanPercentageScore(List<Result> results)
        {
            // if there are no results, mean percentage score can't be calculated
            if (results.Count == 0)
                return double.NaN;

            var numberOfResults = results.Count;
            var totalPercentScore = 0.0;

            foreach (Result result in results)
            {
                totalPercentScore += result.ResultPercentageScore;
            }
            var meanPercentageScore = totalPercentScore / numberOfResults;

            var roundedMeanPercentageScore = Math.Round(meanPercentageScore, 1, MidpointRounding.AwayFromZero);

            return roundedMeanPercentageScore;
        }

        private static double GetRoundedStandardDeviation(List<Result> results, double meanPercentageScore)
        {
            // if there are no results, standard deviation can't be calculated
            if (results.Count == 0)
                return double.NaN;

            // if there is only one result, standard deviation is zero
            if (results.Count == 1)
                return 0;

            // there is more than one result, so lets calculate the standard deviation
            var numberOfResultsAsDouble = (double) results.Count;
            var deviationSquaredSum = 0.0;

            foreach (var result in results)
            {
                var deviation = result.ResultPercentageScore - meanPercentageScore;
                var deviationSquared = deviation * deviation;
                deviationSquaredSum += deviationSquared;
            }
            var deviationSquaredMean = deviationSquaredSum * ( 1 / ( numberOfResultsAsDouble - 1 ) );
            var standardDeviation = Math.Sqrt(deviationSquaredMean);

            var roundedStandardDeviation = Math.Round(standardDeviation, 1, MidpointRounding.AwayFromZero);

            return roundedStandardDeviation;
        }

        private static double GetRoundedMedianPercentageScore(List<Result> results)
        {
            // if there are no results, median percentage score can't be calculated
            if (results.Count == 0)
                return double.NaN;

            var orderedResults = results.OrderBy(r => r.ResultPercentageScore).ToList();

            var numberOfResults = orderedResults.Count;
            var medianPercentageScore = 0.0;

            if (numberOfResults % 2 == 0)
            {
                var firstMiddle = ( numberOfResults / 2 ) - 1;
                var secondMiddle = firstMiddle + 1;
                var firstMiddleScore = orderedResults[firstMiddle].ResultPercentageScore;
                var secondMiddleScore = orderedResults[secondMiddle].ResultPercentageScore;

                medianPercentageScore = ( firstMiddleScore + secondMiddleScore ) / 2.0;
            }
            else
            {
                var middle = numberOfResults / 2;

                medianPercentageScore = orderedResults[middle].ResultPercentageScore;
            }

            var roundedMedianPercentageScore = Math.Round(medianPercentageScore, 1, MidpointRounding.AwayFromZero);

            return roundedMedianPercentageScore;
        }
    }

    internal class QuartileScores
    {
        internal double Quartile0Score { get; set; }
        internal double Quartile1Score { get; set; }
        internal double Quartile2Score { get; set; }
        internal double Quartile3Score { get; set; }
        internal double Quartile4Score { get; set; }

        public QuartileScores()
        {
            // start at non-number, since starting at 0 means there was a result
            Quartile0Score = double.NaN;
            Quartile1Score = double.NaN;
            Quartile2Score = double.NaN;
            Quartile3Score = double.NaN;
            Quartile4Score = double.NaN;
        }
    }
}
