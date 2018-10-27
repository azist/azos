/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
 
  
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using NFX;
using Azos.Apps.Pile;
using Azos.Scripting;

namespace Azos.Tests.Unit.AppModel.Pile
{
  public class MMFPileTestBase : IRunHook
  {
      public const string LOCAL_ROOT = @"c:\NFX\ut-pile";


      bool IRunHook.Prologue(Runner runner, FID id, MethodInfo method, RunAttribute attr, ref object[] args)
      {
        GC.Collect();
        Directory.CreateDirectory(LOCAL_ROOT);
        return false;
      }

      bool IRunHook.Epilogue(Runner runner, FID id, MethodInfo method, RunAttribute attr, Exception error)
      {
        GC.Collect();
        Azos.IOMiscUtils.EnsureDirectoryDeleted(LOCAL_ROOT);
        return false;
      }


      public MMFPile MakeMMFPile()
      {
        var result = new MMFPile("UT");
        result.DataDirectoryRoot = LOCAL_ROOT;
        return result;
      }
  }
}
