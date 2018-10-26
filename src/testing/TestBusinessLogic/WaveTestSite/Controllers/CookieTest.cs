
using System;
using System.Net;

using Azos;
using Azos.Wave.Mvc;

namespace WaveTestSite.Controllers
{

  public class CookieTest : Controller
  {
    [Action]
    public string SetCookies()
    {
      var c1 = new Cookie
      {
        Name="cookieA",
        Value="valueA"
      };

      var c2 = new Cookie
      {
        Name = "cookieB",
        Value = "valueB"
      };

      //WorkContext.Response.AppendCookie(c1);
      //WorkContext.Response.AppendCookie(c2);

      WorkContext.Response.AddHeader("Set-Cookie", "{0}={1}; Expires=Wed, 09 Jun 2021 10:18:14 GMT; Path=/;".Args(c1.Name, c1.Value));
      WorkContext.Response.AddHeader("Set-Cookie-New", "{0}={1}; Expires=Wed, 09 Jun 2021 10:18:14 GMT; Path=/;".Args(c2.Name, c2.Value));
      return "OK!";
    }
  }
}
