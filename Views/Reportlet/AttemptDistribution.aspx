<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="ViewPage<AttemptDistributionModel>" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadPlaceHolder" runat="server">
  <style type="text/css">
    #chart_div table {
      width: auto;
      margin: 0 auto !important;
    }
  </style>
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="BodyPlaceHolder" runat="server">
  
  <div class="row-fluid">
  
    <div class="span12">
    
      <% using (Html.BeginBootstrapHorizontalForm(new { ViewBag.ReturnUrl })) { %>
    
        <%: Html.AntiForgeryToken() %>
    
        &nbsp;
    
        <div class="control-group">
          <%: Html.BootstrapControlLabelFor(m => m.SelectedAssessment) %>
          <div class="controls">
            <%: Html.DropDownListFor(m => m.SelectedAssessment, Model.AssessmentItems) %>
          </div>
        </div>
        <div class="control-group">
          <%: Html.BootstrapControlLabelFor(m => m.ScorebandNameForPass) %>
          <div class="controls">
            <%: Html.TextBoxFor(m => m.ScorebandNameForPass) %>
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
            <td>Total Attempts:&nbsp;</td>
            <td id="TotalAttempts">-</td>
          </tr>
          <tr>
            <td>Mean Attempts:&nbsp;</td>
            <td id="MeanAttempts">-</td>
          </tr>
          <tr>
            <td>Standard Deviation:&nbsp;</td>
            <td id="StandardDeviationAttempts">-</td>
          </tr>
          <tr>
            <td>Median Attempts:&nbsp;</td>
            <td id="MedianAttempts">-</td>
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
              'selectedAssessment': $('#SelectedAssessment').val(),
              'scorebandNameForPass': $('#ScorebandNameForPass').val()
            };

            $('#chart_div').html('<div style="text-align: center; color: red; margin-top: 20px;">loading...</div>');
            $('#TotalAttempts').text('-');
            $('#MeanAttempts').text('-');
            $('#StandardDeviationAttempts').text('-');
            $('#MedianAttempts').text('-');

            ajaxCall = $.ajax({
              type: 'GET',
              url: '/Reportlet/AttemptDistributionData/',
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
      var highestResultCount = 0;
      if (jsonData.hasOwnProperty('highestResultCount')) {
        highestResultCount = jsonData.highestResultCount;
      }
      var vAxisMaxValue = (Math.floor(highestResultCount / 10) + 1) * 10;

      if (jsonData.hasOwnProperty("chartData")) {

        var selectedAssessmentName = $('#SelectedAssessment option:selected').text();

        var dataTable = new google.visualization.DataTable();
        dataTable.addColumn('string', '');
        dataTable.addColumn('number', '');
        dataTable.addRows(jsonData.chartData);

        var options = {
          bar: {
            groupWidth: '95%'
          },
          hAxis: {
            slantedText: true,
            title: 'Attempt Number'
          },
          legend: {
            position: 'none'
          },
          title: 'Attempt Distribution for: ' + selectedAssessmentName,
          vAxis: {
            gridlines: {
              count: 6
            },
            minValue: 0,
            maxValue: vAxisMaxValue,
            title: 'Number of Results'
          }
        };

        var chart = new google.visualization.ColumnChart(document.getElementById('chart_div'));
        chart.draw(dataTable, options);
      }
    }

    function updateResultsTable(jsonData) {
      if (jsonData.hasOwnProperty("totalAttempts")) {
        $('#TotalAttempts').text(jsonData.totalAttempts);
      }
      if (jsonData.hasOwnProperty("meanAttempts")) {
        $('#MeanAttempts').text(jsonData.meanAttempts);
      }
      if (jsonData.hasOwnProperty("standardDeviationAttempts")) {
        $('#StandardDeviationAttempts').text(jsonData.standardDeviationAttempts);
      }
      if (jsonData.hasOwnProperty("medianAttempts")) {
        $('#MedianAttempts').text(jsonData.medianAttempts);
      }
    }
  </script>
</asp:Content>