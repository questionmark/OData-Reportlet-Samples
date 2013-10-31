using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace QM.Reporting.ODataDashboard.Web.Extensions
{
    public static class ModelExtensions
    {
        /// <summary>
        /// Creates a list of SelectListItems, used by the DropdownListFor() html helper method to 
        /// create html select tags (aka dropdown lists)
        /// </summary>
        public static IEnumerable<SelectListItem> ToListItems<T>(this List<T> items, Func<T, string> pickValue, Func<T, string> pickText, Func<T, bool> pickSelected = null, string defaultValue = "0", string defaultText = "-- select --")
        {
            yield return new SelectListItem
            {
                Value = defaultValue,
                Text = defaultText
            };

            if (items != null)
            {
                foreach (var item in items)
                    yield return new SelectListItem
                    {
                        Value = pickValue(item),
                        Text = pickText(item),
                        Selected = pickSelected != null && pickSelected(item)
                    };
            }
        }
    }
}