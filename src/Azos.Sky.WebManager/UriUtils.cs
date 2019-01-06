/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System.IO;

namespace Azos.Sky.WebManager
{
  /// <summary>
  /// Centralizes site URI static resource paths management
  /// </summary>
  public static class SURI
  {
    public const string STATIC =  "/static/site/";
    public const string IMG    =  STATIC + "img/";
    public const string STL    =  STATIC + "stl/";
    public const string SCR    =  STATIC + "scr/";
    public const string STOCK  =  "/static/stock/site/";
    public const string STOCK_STL  =  STOCK + "stl/";

    public static string Image(string path)
    {
      return Path.Combine(IMG, path);
    }

    public static string Style(string path)
    {
      return Path.Combine(STL, path);
    }

    public static string StockStyle(string path)
    {
      return Path.Combine(STOCK_STL, path);
    }

    public static string Script(string path)
    {
      return Path.Combine(SCR, path);
    }

    public static string Stock(string path)
    {
      return Path.Combine(STOCK, path);
    }

  }

  /// <summary>
  /// Centralizes main site URIs that lead to dynamic pages
  /// </summary>
  public static class URIS
  {
    public const string HOME   = "/";
    public const string MVC = "/mvc";

    public const string CONSOLE = "/console";

    public const string INSTRUMENTATION  = "/instrumentation";
    public const string INSTRUMENTATION_MVC = MVC + INSTRUMENTATION;

    public const string INSTRUMENTATION_CHARTS = "/instrumentation-charts";
    public const string INSTRUMENTATION_LOGS = "/instrumentation-logs";

    public const string THE_SYSTEM  = "/thesystem";
    public const string THE_SYSTEM_MVC = MVC + THE_SYSTEM;

    public const string PUB_API_HOST_PERFORMANCE  = "/pub-api/hostperformance";

    public const string PROCESS_MANAGER = "/processmanager";
    public const string PROCESS_MANAGER_MVC = MVC + PROCESS_MANAGER;

  }

}
