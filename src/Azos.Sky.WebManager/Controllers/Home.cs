using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Wave;
using Azos.Security;
using Azos.Wave.Mvc;
using Azos.Data;

namespace Azos.Sky.WebManager.Controllers
{
  /// <summary>
  /// Web manager Home comntroller
  /// </summary>
  public sealed class Home : WebManagerController
  {

    [Action]
    public object Index()
    {
      return Localizer.MakePage<Pages.Process>();
    }

    [Action]
    public object Console()
    {
      return Localizer.MakePage<Pages.Console>();
    }

    [Action]
    public object Login(string id, string pwd)
    {
      WorkContext.NeedsSession();
      var session = WorkContext.Session;


      if (session==null || id.IsNullOrWhiteSpace())
        return Localizer.MakePage<Pages.Login>("");


      if (session.User.Status==UserStatus.Invalid)
      {
        var cred = new IDPasswordCredentials(id, pwd);
        var user = App.SecurityManager.Authenticate(cred);
        if (user.Status==UserStatus.Invalid)
          return Localizer.MakePage<Pages.Login>("Invalid login");

        WorkContext.Session.User = user;
      }
      return new Redirect("/");
    }

    [Action]
    public object Logout()
    {
      var session = WorkContext.Session;
      if (session!=null) session.User = null;

      return Localizer.MakePage<Pages.Login>("");
    }

    [Action]
    public object TheSystem()
    {
      return Localizer.MakePage<Pages.TheSystem>();
    }

    [Action]
    public object Instrumentation()
    {
      return Localizer.MakePage<Pages.Instrumentation>();
    }

    [Action]
    public object ProcessManager(string zone)
    {
      return Localizer.MakePage<Pages.ProcessManager>(zone);
    }

    [Action("instrumentation-charts", 20)]
    public object InstrumentationCharts()
    {
      return Localizer.MakePage<Pages.InstrumentationCharts>();
    }

    [Action("instrumentation-logs", 30)]
    public object InstrumentationLogs()
    {
      return Localizer.MakePage<Pages.InstrumentationLogs>();
    }
  }
}
