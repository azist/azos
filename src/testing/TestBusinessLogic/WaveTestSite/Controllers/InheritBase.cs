/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Wave.Mvc;

namespace WaveTestSite.Controllers
{
  /// <summary>
  /// Adds numbers
  /// </summary>
  public class InheritBase : Controller
  {

    [Action]
    public string Echo1(string msg)
    {
      return "Base:Echo1 "+msg;
    }

    [Action]
    public string EchoNew(string msg)
    {
      return "Base:EchoNew "+msg;
    }

    [Action]
    public virtual string EchoVirtual(string msg)
    {
      return "Base:EchoVirtual "+msg;
    }
  }


}
