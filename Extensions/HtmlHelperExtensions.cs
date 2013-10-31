using System;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;

namespace QM.Reporting.ODataDashboard.Web.Extensions
{
    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// Creates form tag with class "form-horizontal", which is a Bootstrap class
        /// </summary>
        public static MvcForm BeginBootstrapHorizontalForm(this HtmlHelper htmlHelper, object routeValues)
        {
            var horizontalForm = htmlHelper.BeginForm(null, null, routeValues, FormMethod.Post, new { @class = "form-horizontal" });

            return horizontalForm;
        }

        /// <summary>
        /// Creates label tag with class "control-label", which is a Bootstrap class
        /// </summary>
        public static MvcHtmlString BootstrapControlLabelFor<T1, T2>(this HtmlHelper<T1> htmlHelper, Expression<Func<T1, T2>> expression, object htmlAttributes = null)
        {
            var modifiedHtmlAttributes = htmlAttributes.AddClass("control-label");

            var label = htmlHelper.LabelFor(expression, modifiedHtmlAttributes);

            return label;
        }

        /// <summary>
        /// Creates checkbox tag with class "checkbox", which is a Bootstrap class
        /// </summary>
        public static MvcHtmlString BootstrapCheckboxFor<T>(this HtmlHelper<T> htmlHelper, Expression<Func<T, bool>> expression, object htmlAttributes = null)
        {
            var modifiedHtmlAttributes = htmlAttributes.AddClass("checkbox");

            var checkbox = htmlHelper.CheckBoxFor(expression, modifiedHtmlAttributes);

            return checkbox;
        }

        private static RouteValueDictionary AddClass(this object htmlAttributes, string classToAdd)
        {
            htmlAttributes = htmlAttributes ?? new object();

            var modifiedHtmlAttributes = new RouteValueDictionary(htmlAttributes);

            if (modifiedHtmlAttributes.ContainsKey("class"))
            {
                modifiedHtmlAttributes["class"] += " " + classToAdd;
            }
            else
            {
                modifiedHtmlAttributes.Add("class", classToAdd);
            }

            return modifiedHtmlAttributes;
        }
    }
}