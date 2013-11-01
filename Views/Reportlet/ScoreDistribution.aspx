<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="ViewPage<ScoreDistributionModel>" %>

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
            <td>Number of Results:&nbsp;</td>
            <td id="NumberOfResults">-</td>
          </tr>
          <tr>
            <td>Mean Percentage Score:&nbsp;</td>
            <td id="MeanPercentageScore">-</td>
          </tr>
          <tr>
            <td>Standard Deviation:&nbsp;</td>
            <td id="StandardDeviation">-</td>
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

        $('#SelectedAssessment').change(
          function () {
            
            if (ajaxCall != null) {
              ajaxCall.abort();
              ajaxCall = null;
            }

            var dataToSend = {
              'selectedAssessment': $('#SelectedAssessment').val()
            };

            $('#chart_div').html('<div style="text-align: center; color: red; margin-top: 20px;">loading...</div>');
            $('#NumberOfResults').text('-');
            $('#MeanPercentageScore').text('-');
            $('#StandardDeviation').text('-');

            ajaxCall = $.ajax({
              type: 'GET',
              url: '/Reportlet/ScoreDistributionData/',
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
            title: 'Score'
          },
          legend: {
            position: 'none'
          },
          title: 'Score Distribution for: ' + selectedAssessmentName,
          vAxis: {
            format: '',
            gridlines: {
              count: 6
            },
            minValue: 0,
            maxValue: vAxisMaxValue,
            title: 'Number of Participants'
          }
        };

        var chart = new google.visualization.ColumnChart(document.getElementById('chart_div'));
        chart.draw(dataTable, options);
      }
    }
    
    function updateResultsTable(jsonData) {
      if (jsonData.hasOwnProperty("numberOfResults")) {
        $('#NumberOfResults').text(jsonData.numberOfResults);
      }
      if (jsonData.hasOwnProperty("meanPercentageScore")) {
        $('#MeanPercentageScore').text(jsonData.meanPercentageScore);
      }
      if (jsonData.hasOwnProperty("standardDeviation")) {
        $('#StandardDeviation').text(jsonData.standardDeviation);
      }
    }
  </script>
</asp:Content>