using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Conf;
using Azos.Sky.Apps;

namespace Azos.Sky.WebManager
{

  /// <summary>
  /// Builds HTML markup for menu configured under WEB-MANAGER config
  /// </summary>
  internal class MenuBuilder
  {

     public const string CONFIG_MENU_SECTION = "menu";
     public const string CONFIG_ITEM_SECTION = "item";
     public const string CONFIG_TEXT_ATTR = "text";
     public const string CONFIG_HREF_ATTR = "href";

     /// <summary>
     /// If there is menu configured, returns it, or empty string.
     /// Path isoLang iso code for localized descriptions, or null for english
     /// Menu structure
     ///  item
     ///  {
     ///     text="american english text" text_fr="french text" text_deu="german text" href="/item path"
     ///     item{...}
     ///  }
     /// </summary>
     public static string BuildMenu(string isoLang)
     {
       var menu = App.ConfigRoot[SkyApplication.CONFIG_WEB_MANAGER_SECTION][CONFIG_MENU_SECTION];
       if (!menu.Exists) return string.Empty;
       var result = new StringBuilder();

       result.AppendLine("<ul>");
         buildMenuLevel(isoLang, result, menu);
       result.AppendLine("</ul>");

       return result.ToString();
     }


     private static void buildMenuLevel(string isoLang, StringBuilder sb, IConfigSectionNode level)
     {
       sb.AppendLine("<li>");

       string text = null;

       if (isoLang.IsNotNullOrWhiteSpace())
         text = level.AttrByName(CONFIG_TEXT_ATTR+"_"+isoLang).ValueAsString();

       if (text.IsNullOrWhiteSpace())
         text = level.AttrByName(CONFIG_TEXT_ATTR).ValueAsString();

       if (text.IsNotNullOrWhiteSpace())
       {
         var href = level.AttrByName(CONFIG_HREF_ATTR).ValueAsString();
         if (href.IsNotNullOrWhiteSpace())
          sb.AppendLine("<a href='{0}'>{1}</a>".Args(href, text));
         else
          sb.AppendLine(text);
       }


       var children = level.Children.Where(cn=>cn.IsSameName(CONFIG_ITEM_SECTION));

       if (children.Any())
         sb.AppendLine("<ul>");

       foreach(var item in children)
           buildMenuLevel(isoLang, sb, item);

       if (children.Any())
         sb.AppendLine("</ul>");

       sb.AppendLine("</li>");

     }


  }
}
