using System;
using System.Threading.Tasks;

using Azos.Wave.Mvc;

namespace Azos.Tests.Unit.Wave.Controllers
{
  public class Asynchronous : Controller
  {
    [Action]
    public async Task<string> ActionPlainText()
    {
      await Task.Yield();
      return "Response in plain text";
    }

    [Action]
    public async Task<object> ActionObjectLiteral()
    {
      await Task.Yield();
      return await getData();
    }

    private async Task<object> getData()
    {
      await Task.Delay(100);
      return new { a = 1, b = true, d = new DateTime(1980, 1, 1) };
    }
  }
}
