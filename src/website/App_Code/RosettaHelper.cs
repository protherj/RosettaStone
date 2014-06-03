﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

/// <summary>
/// This is a utility class solely used for presentation of the Merchello Rosetta Stone project site.  It has
/// no relation to Merchello what so ever.
/// </summary>
public class RosettaHelper
{

    /// <summary>
    /// Utility to create html snippet to describe the views used on the current page
    /// </summary>
    public static MvcHtmlString GetViewBoxHtml(string viewName, string description)
    {
        return GetViewBoxHtml(viewName, description, new string[] {});
    }

    /// <summary>
    /// Utility to create html snippet to describe the views used on the current page
    /// </summary>
	public static MvcHtmlString GetViewBoxHtml(string viewName, string description, IEnumerable<string> merchelloSpecificCode)
	{
        const string html = @"<div class=""list-group-item"">
                    <h4 class=""list-group-item-heading"">{0}</h4>
                    <p class=""list-group-item-text"" style=""margin-bottom:10px;"">{1}</p>
                    {2}
                </div>";

	    return MvcHtmlString.Create(string.Format(html, viewName, description, BuildMerchelloSpecificCodeList(merchelloSpecificCode)));
	}

    private static string BuildMerchelloSpecificCodeList(IEnumerable<string> merchelloSpecificCode)
    {
        var specificCode = merchelloSpecificCode as string[] ?? merchelloSpecificCode.ToArray();
        if (!specificCode.Any()) return string.Format("<p class=\"bg-warning\"><i>No Merchello-specific code, this is strictly an Umbraco view.</i></p>");

        var sb = new StringBuilder();

        sb.Append("<div class=\"bg-warning\">");
        sb.Append("<h5>Merchello Specific Code:</h5>");
        sb.Append("<ul>");
        
        foreach (var tidbit in specificCode)
        {
            sb.AppendFormat("<li>{0}</li>", tidbit);
        }
        sb.Append("</ul></div>");

        return sb.ToString();
    }

    public static string InheritsMerchelloViewPage = "<b>Inherits</b> <abbr title=\"Exposes the CustomerContext and MerchelloHelper in a view with a custom model\">Merchello.Web.Mvc.MerchelloViewPage<{0}></abbr>";
    public static string InheritsMerchelloTemplatePage = "<b>Inherits</b> <abbr title=\"Exposes the CustomerContext and MerchelloHelper\">Merchello.Web.Mvc.MerchelloTemplatePage</abbr>";
    public static string UsingMerchelloWeb = "<b>Using</b> <abbr title=\"Typically included to expose extensions and certain Merchello types such as the Basket\">Merchello.Web</abbr>";
}