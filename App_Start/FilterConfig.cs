using System.Web.Mvc;

namespace QM.Reporting.ODataDashboard.Web.App_Start
{
  public class FilterConfig
  {
    public static void RegisterGlobalFilters(GlobalFilterCollection filters)
    {
      filters.Add(new HandleErrorAttribute());
    }
  }
}