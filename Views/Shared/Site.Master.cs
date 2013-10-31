using System;
using System.Web.UI;

namespace QM.Reporting.ODataDashboard.Web.Views.Shared
{
  public partial class Site : MasterPage
  {
      protected override void OnLoad(EventArgs e)
      {
          base.OnLoad(e);
          Page.Header.DataBind();
      }
  }
}