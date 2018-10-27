/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using Azos.Scripting;
using System;


using Azos.Web;
using Azos.Data;

namespace Azos.Tests.Unit.Web
{
  [Runnable(TRUN.BASE)]
  public class CacheControlTests
  {
    [Run]
    public void CacheControlConfig()
    {
      var conf = @"
cache-control {
  cacheability=public
  max-age-sec=60
  shared-max-age-sec=120

  no-store=true
  no-transform=true
  must-revalidate=true
  proxy-revalidate=true
}".AsLaconicConfig(handling: ConvertErrorHandling.Throw);

      var cc = CacheControl.FromConfig(conf);

      Aver.IsTrue(CacheControl.Type.Public == cc.Cacheability);
      Aver.AreEqual(60, cc.MaxAgeSec);
      Aver.AreEqual(120, cc.SharedMaxAgeSec);
      Aver.IsTrue(cc.NoStore);
      Aver.IsTrue(cc.NoTransform);
      Aver.IsTrue(cc.MustRevalidate);
      Aver.IsTrue(cc.ProxyRevalidate);

      Console.WriteLine(cc.HTTPCacheControl);
      Aver.IsTrue(cc.HTTPCacheControl.EqualsOrdIgnoreCase("public, no-transform, max-age=60, s-maxage=120, must-revalidate, proxy-revalidate"));
    }
  }
}
