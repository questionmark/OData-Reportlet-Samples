<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadPlaceHolder" runat="server">
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="BodyPlaceHolder" runat="server">
  
  <div class="container">

    <div class="span12">

      <h1>OData Reportlet Samples</h1>

      <ul class="nav nav-pills">
        <li>
          <a href="/Reportlet/AttemptDistribution/">
            Attempt Distribution
          </a>
        </li>
        <li>
          <a href="/Reportlet/PreTestPostTest/">
            PreTest PostTest
          </a>
        </li>
        <li>
          <a href="/Reportlet/ScoreCorrelation/">
            Score Correlation
          </a>
        </li>
        <li>
          <a href="/Reportlet/ScoreDistribution/">
            Score Distribution
          </a>
        </li>
      </ul>
    </div>
  </div>

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptPlaceHolder" runat="server">
</asp:Content>
