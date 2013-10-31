<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="ViewPage<ScoreCorrelationModel>" %>

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
          <%: Html.BootstrapControlLabelFor(m => m.FirstSelectedAssessment) %>
          <div class="controls">
            <%: Html.DropDownListFor(m => m.FirstSelectedAssessment, Model.AssessmentItems) %>
          </div>
        </div>
        <div class="control-group">
          <%: Html.BootstrapControlLabelFor(m => m.SecondSelectedAssessment) %>
          <div class="controls">
            <%: Html.DropDownListFor(m => m.SecondSelectedAssessment, Model.AssessmentItems) %>
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
              'firstSelectedAssessment': $('#FirstSelectedAssessment').val(),
              'secondSelectedAssessment': $('#SecondSelectedAssessment').val()
            };

            $('#chart_div').html('<div style="text-align: center; color: red; margin-top: 20px;">loading...</div>');

            ajaxCall = $.ajax({
              type: 'GET',
              url: '../ScoreCorrelationData/',
              data: dataToSend,
              datatype: 'json',

              success: function (data) {
                ajaxCall = null;
                var jsonData = $.parseJSON(data);
                drawChart(jsonData);
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

        var firstSelectedAssessmentName = $('#FirstSelectedAssessment option:selected').text();
        var secondSelectedAssessmentName = $('#SecondSelectedAssessment option:selected').text();
        
        var dataTable = new google.visualization.DataTable();
        dataTable.addColumn('number', '');
        dataTable.addColumn('number', '');
        dataTable.addColumn({ type : 'string', role : 'tooltip'});
        dataTable.addRows(jsonData.chartData);

        var options = {
          hAxis: {
            format: '###',
            minValue: 0,
            maxValue:100,
            slantedText: true,
            ticks: [0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100],
            title: firstSelectedAssessmentName
          },
          legend: {
            position: 'none'
          },
          title: 'Score Correlation between ' + firstSelectedAssessmentName + ' and ' + secondSelectedAssessmentName,
          vAxis: {
            format: '###',
            minValue: 0,
            maxValue: 100,
            ticks: [0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100],
            title: secondSelectedAssessmentName
          }
        };

        var chart = new google.visualization.ScatterChart(document.getElementById('chart_div'));
        chart.draw(dataTable, options);
      }
    }
  </script>
</asp:Content>