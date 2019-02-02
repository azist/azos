/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using Azos.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Web;
using Azos.Serialization.JSON;

namespace Azos.Tests.Unit.Web
{
  [Runnable(TRUN.BASE, 6)]
  public class ContentTypeMappingsTests
  {
    [Run]
    public void DefaultMappings()
    {
      using(var app = new AzosApplication(null, null))
      {
        var maps = app.GetContentTypeMappings();
        Aver.IsNotNull(maps);

        var map = maps.MapFileExtension("txt");
        Aver.IsNotNull(map);
        Aver.IsFalse(map.IsGeneric);
        Aver.IsFalse(map.IsBinary);
        Aver.IsTrue(map.IsText);
        Aver.IsFalse(map.IsImage);
        Aver.AreEqual("text/plain", map.ContentType);

        map = maps.MapFileExtension("htm");
        Aver.IsNotNull(map);
        Aver.IsFalse(map.IsGeneric);
        Aver.IsFalse(map.IsBinary);
        Aver.IsTrue(map.IsText);
        Aver.IsFalse(map.IsImage);
        Aver.AreEqual("text/html", map.ContentType);

        map = maps.MapFileExtension("png");
        Aver.IsNotNull(map);
        Aver.IsFalse(map.IsGeneric);
        Aver.IsTrue(map.IsBinary);
        Aver.IsFalse(map.IsText);
        Aver.IsTrue(map.IsImage);
        Aver.AreEqual("image/png", map.ContentType);
        Console.WriteLine(map.ToJSON());
      }
    }
  }
}
