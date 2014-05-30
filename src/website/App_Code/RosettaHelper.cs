using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

/// <summary>
/// Summary description for RosettaHelper
/// </summary>
public class RosettaHelper
{
    /// <summary>
    /// Utility to create html snippet to describe the views used on the current page
    /// </summary>
	public static MvcHtmlString GetViewBoxHtml(string viewName, string description)
	{
        const string html = @"<div class=""list-group-item"">
                    <h3 class=""list-group-item-heading"">{0}</h3>
                    <p class=""list-group-item-text"">{1}</p>    
                </div>";

	    return MvcHtmlString.Create(string.Format(html, viewName, description));
	}
}