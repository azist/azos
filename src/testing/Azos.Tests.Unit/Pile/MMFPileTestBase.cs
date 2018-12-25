/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;
using System.IO;
using System.Reflection;

using Azos.Apps;
using Azos.Pile;
using Azos.Scripting;

namespace Azos.Tests.Unit.Pile
{
  public class MMFPileTestBase : IRunHook
  {
      #warning Avoid local constants - use vars + move into integration tests
      public const string LOCAL_ROOT = @"c:\Azos\ut-pile";


      bool IRunHook.Prologue(Runner runner, FID id, MethodInfo method, RunAttribute attr, ref object[] args)
      {
        GC.Collect();
        Directory.CreateDirectory(LOCAL_ROOT);
        return false;
      }

      bool IRunHook.Epilogue(Runner runner, FID id, MethodInfo method, RunAttribute attr, Exception error)
      {
        GC.Collect();
        Azos.IOUtils.EnsureDirectoryDeleted(LOCAL_ROOT);
        return false;
      }


      public MMFPile MakeMMFPile()
      {
        var result = new MMFPile(NOPApplication.Instance);
        result.DataDirectoryRoot = LOCAL_ROOT;
        return result;
      }
  }
}
