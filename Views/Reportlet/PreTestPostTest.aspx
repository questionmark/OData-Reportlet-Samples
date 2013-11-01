<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="ViewPage<PreTestPostTestModel>" %>
<%@ Import Namespace="QM.Reporting.ODataDashboard.Web.Enums" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadPlaceHolder" runat="server">
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="BodyPlaceHolder" runat="server">
  
  <div class="row-fluid">
  
    <div class="span12">
    
      <% using (Html.BeginBootstrapHorizontalForm(new { ViewBag.ReturnUrl })) { %>
    
        <%: Html.AntiForgeryToken() %>
    
        &nbsp;
    
        <div class="control-group">
          <div class="controls-row">
            <%: Html.BootstrapControlLabelFor(m => m.FirstAssessment) %>
            <div class="controls">
              <%: Html.DropDownListFor(m => m.FirstAssessment, Model.AssessmentItems) %>
            </div>
          </div>
          <div class="controls-row">
            <%: Html.BootstrapControlLabelFor(m => m.FirstAssessmentAttemptToUse) %>
            <div class="controls">
              <%: Html.RadioButtonFor(m => m.FirstAssessmentAttemptToUse, Attempt.First, new { id = "FirstAssessmentAttemptToUse.First"}) %>
              <label class="radio inline" for="FirstAssessmentAttemptToUse.First">First</label><br />
              <%: Html.RadioButtonFor(m => m.FirstAssessmentAttemptToUse, Attempt.Last, new { id = "FirstAssessmentAttemptToUse.Last"}) %>
              <label class="radio inline" for="FirstAssessmentAttemptToUse.Last">Last</label><br />
              <%: Html.RadioButtonFor(m => m.FirstAssessmentAttemptToUse, Attempt.Number, new { id = "FirstAssessmentAttemptToUse.Number"}) %>
              <label class="radio inline" for="FirstAssessmentAttemptToUse.Number">Number</label>
            </div>
          </div>
          <div class="controls-row">
            <%: Html.BootstrapControlLabelFor(m => m.FirstAssessmentAttemptNumber) %>
            <div class="controls">
              <%: Html.TextBoxFor(m => m.FirstAssessmentAttemptNumber) %>
            </div>
          </div>
        </div>
        <div class="control-group">
          <div class="controls-row">
            <%: Html.BootstrapControlLabelFor(m => m.SecondAssessment) %>
            <div class="controls">
              <%: Html.DropDownListFor(m => m.SecondAssessment, Model.AssessmentItems) %>
            </div>
          </div>
          <div class="controls-row">
            <%: Html.BootstrapControlLabelFor(m => m.SecondAssessmentAttemptToUse) %>
            <div class="controls">
              <%: Html.RadioButtonFor(m => m.SecondAssessmentAttemptToUse, Attempt.First, new { id = "SecondAssessmentAttemptToUse.First"}) %>
              <label class="radio inline" for="SecondAssessmentAttemptToUse.Second">First</label><br />
              <%: Html.RadioButtonFor(m => m.SecondAssessmentAttemptToUse, Attempt.Last, new { id = "SecondAssessmentAttemptToUse.Last"}) %>
              <label class="radio inline" for="SecondAssessmentAttemptToUse.Last">Last</label><br />
              <%: Html.RadioButtonFor(m => m.SecondAssessmentAttemptToUse, Attempt.Number, new { id = "SecondAssessmentAttemptToUse.Number"}) %>
              <label class="radio inline" for="SecondAssessmentAttemptToUse.Number">Number</label>
            </div>
          </div>
          <div class="controls-row">
            <%: Html.BootstrapControlLabelFor(m => m.SecondAssessmentAttemptNumber) %>
            <div class="controls">
              <%: Html.TextBoxFor(m => m.SecondAssessmentAttemptNumber) %>
            </div>
          </div>
        </div>
        <div class="control-group">
          <div class="controls">
            <input type="button" value="Submit" class="btn" id="SubmitButton" />
          </div>
        </div>

      <% } %>
    
    </div>

  </div>

  <div style="border: solid 1px lightgray;">

    <div class="row-fluid">
    
      <div id="chart_div" class="span12" style="height: 500px;"></div>

    </div>
    
    <div class="row-fluid">
    
      <div id="table_div" class="span12">
        <table class="table-hover table-bordered">
          <tr>
            <th>&nbsp;</th>
            <th>First Assessment</th>
            <th>Second Assessment</th>
          </tr>
          <tr>
            <td>Number of Results:&nbsp;</td>
            <td id="CountOfFilteredResultsForFirstAssessment">-</td>
            <td id="CountOfFilteredResultsForSecondAssessment">-</td>
          </tr>
          <tr>
            <td>Ignored Results:&nbsp;</td>
            <td id="CountOfIgnoredResultsForFirstAssessment">-</td>
            <td id="CountOfIgnoredResultsForSecondAssessment">-</td>
          </tr>
          <tr>
            <td>Mean Score:&nbsp;</td>
            <td id="MeanScoreOfFilteredResultsForFirstAssessment">-</td>
            <td id="MeanScoreOfFilteredResultsForSecondAssessment">-</td>
          </tr>
          <tr>
            <td>Standard Deviation:&nbsp;</td>
            <td id="StandardDeviationOfFilteredResultsForFirstAssessment">-</td>
            <td id="StandardDeviationOfFilteredResultsForSecondAssessment">-</td>
          </tr>
          <tr>
            <td>Median Score:&nbsp;</td>
            <td id="MedianScoreOfFilteredResultsForFirstAssessment">-</td>
            <td id="MedianScoreOfFilteredResultsForSecondAssessment">-</td>
          </tr>
        </table>
      </div>
    
    </div>
    
  </div>

</asp:Content>

<asp:Content ID="ScriptContent" ContentPlaceHolderID="ScriptPlaceHolder" runat="server">
  <script type="text/javascript" src="https://www.google.com/jsapi"></script>
  <script type="text/javascript">
    google.load("visualization", "1", { packages: ["corechart"] });
    
    // set up form
    $(
      function () {
        setEnabledStatus('FirstAssessment');
        setEnabledStatus('SecondAssessment');

        $('input:radio[name=FirstAssessmentAttemptToUse]').change(
          function() {
            setEnabledStatus('FirstAssessment');
          }
        );
        $('input:radio[name=SecondAssessmentAttemptToUse]').change(
          function() {
            setEnabledStatus('SecondAssessment');
          }
        );
      }
    );
    
    function setEnabledStatus(assessment) {
      var selectedAttemptToUse = $('input:radio[name=' + assessment + 'AttemptToUse]:checked').val();
      if (selectedAttemptToUse == "Number") {
        // enable and select the text
        $('#' + assessment + 'AttemptNumber').removeAttr("disabled");
        $('#' + assessment + 'AttemptNumber').select();
      } else {
        // disable
        $('#' + assessment + 'AttemptNumber').attr("disabled", true);
      }
    }

    $(
      function () {
        var ajaxCall = null;

        $('#SubmitButton').click(
          function () {

            if (ajaxCall != null) {
              ajaxCall.abort();
              ajaxCall = null;
            }

            var dataToSend = {
              'firstAssessment': $('#FirstAssessment').val(),
              'firstAssessmentAttemptToUse': $('input:radio[name=FirstAssessmentAttemptToUse]:checked').val(),
              'firstAssessmentAttemptNumber': $('#FirstAssessmentAttemptNumber').val(),
              'secondAssessment': $('#SecondAssessment').val(),
              'secondAssessmentAttemptToUse': $('input:radio[name=SecondAssessmentAttemptToUse]:checked').val(),
              'secondAssessmentAttemptNumber': $('#SecondAssessmentAttemptNumber').val(),
            };

            $('#chart_div').html('<div style="text-align: center; color: red; margin-top: 20px;">loading...</div>');
            $('#CountOfFilteredResultsForFirstAssessment').text("-");
            $('#CountOfIgnoredResultsForFirstAssessment').text("-");
            $('#MeanScoreOfFilteredResultsForFirstAssessment').text("-");
            $('#StandardDeviationOfFilteredResultsForFirstAssessment').text("-");
            $('#MedianScoreOfFilteredResultsForFirstAssessment').text("-");
            $('#CountOfFilteredResultsForSecondAssessment').text("-");
            $('#CountOfIgnoredResultsForSecondAssessment').text("-");
            $('#MeanScoreOfFilteredResultsForSecondAssessment').text("-");
            $('#StandardDeviationOfFilteredResultsForSecondAssessment').text("-");
            $('#MedianScoreOfFilteredResultsForSecondAssessment').text("-");

            ajaxCall = $.ajax({
              type: 'GET',
              url: '/Reportlet/PreTestPostTestData/',
              data: dataToSend,
              datatype: 'json',

              success: function (data) {
                ajaxCall = null;
                var jsonData = $.parseJSON(data);
                drawChart(jsonData);
                updateResultsTable(jsonData);
              },

              error: function (response, status) {
                ajaxCall = null;
                if (status != 'abort') {
                  $('#chart_div').html('<div style="text-align: center; color: red; margin-top: 20px;">error: ' + response.responseText + '</div>');
                }
              }
            });
          });
      });

    function drawChart(jsonData) {
      if (jsonData.hasOwnProperty("chartData")) {

        var dataTable = new google.visualization.DataTable();
        dataTable.addColumn('string', '');
        dataTable.addColumn('number', '');
        dataTable.addColumn('number', '');
        dataTable.addColumn('number', '');
        dataTable.addColumn('number', '');
        dataTable.addRows(jsonData.chartData);

        var options = {
          bar: {
            groupWidth: '50%'
          },
          hAxis: {
            title: 'Assessments'
          },
          legend: {
            position: 'none'
          },
          title: 'Pre-Test Post-Test Results',
          vAxis: {
            gridlines: {
              count: 6
            },
            minValue: 0,
            maxValue: 100,
            title: 'Result Scores'
          }
        };

        var chart = new google.visualization.CandlestickChart(document.getElementById('chart_div'));
        chart.draw(dataTable, options);
      }
    }

    function updateResultsTable(jsonData) {
      if (jsonData.hasOwnProperty("countOfFilteredResultsForFirstAssessment")) {
        $('#CountOfFilteredResultsForFirstAssessment').text(jsonData.countOfFilteredResultsForFirstAssessment);
      }
      if (jsonData.hasOwnProperty("countOfIgnoredResultsForFirstAssessment")) {
        $('#CountOfIgnoredResultsForFirstAssessment').text(jsonData.countOfIgnoredResultsForFirstAssessment);
      }
      if (jsonData.hasOwnProperty("meanScoreOfFilteredResultsForFirstAssessment")) {
        $('#MeanScoreOfFilteredResultsForFirstAssessment').text(jsonData.meanScoreOfFilteredResultsForFirstAssessment);
      }
      if (jsonData.hasOwnProperty("standardDeviationOfFilteredResultsForFirstAssessment")) {
        $('#StandardDeviationOfFilteredResultsForFirstAssessment').text(jsonData.standardDeviationOfFilteredResultsForFirstAssessment);
      }
      if (jsonData.hasOwnProperty("medianScoreOfFilteredResultsForFirstAssessment")) {
        $('#MedianScoreOfFilteredResultsForFirstAssessment').text(jsonData.medianScoreOfFilteredResultsForFirstAssessment);
      }
      if (jsonData.hasOwnProperty("countOfFilteredResultsForSecondAssessment")) {
        $('#CountOfFilteredResultsForSecondAssessment').text(jsonData.countOfFilteredResultsForSecondAssessment);
      }
      if (jsonData.hasOwnProperty("countOfIgnoredResultsForSecondAssessment")) {
        $('#CountOfIgnoredResultsForSecondAssessment').text(jsonData.countOfIgnoredResultsForSecondAssessment);
      }
      if (jsonData.hasOwnProperty("meanScoreOfFilteredResultsForSecondAssessment")) {
        $('#MeanScoreOfFilteredResultsForSecondAssessment').text(jsonData.meanScoreOfFilteredResultsForSecondAssessment);
      }
      if (jsonData.hasOwnProperty("standardDeviationOfFilteredResultsForSecondAssessment")) {
        $('#StandardDeviationOfFilteredResultsForSecondAssessment').text(jsonData.standardDeviationOfFilteredResultsForSecondAssessment);
      }
      if (jsonData.hasOwnProperty("medianScoreOfFilteredResultsForSecondAssessment")) {
        $('#MedianScoreOfFilteredResultsForSecondAssessment').text(jsonData.medianScoreOfFilteredResultsForSecondAssessment);
      }
    }
  </script>
</asp:Content>