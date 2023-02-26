/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Azos.CodeAnalysis;
using Azos.CodeAnalysis.JSON;
using Azos.CodeAnalysis.Source;

namespace Azos.Serialization.JSON.Backends
{
  internal sealed class FsmCached
  {
    private static FsmCached s_1;
    private static FsmCached s_2;
    private static FsmCached s_3;
    private static FsmCached s_4;


    public FsmCached Get()
    {
      var result = Interlocked.Exchange(ref s_1, null);
      return null;
    }


  }
}
