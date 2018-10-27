/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Azos;
using Azos.Conf;
using Azos.Wave;
using Azos.Wave.Handlers;

namespace WaveTestSite.Handlers
{
  public class SimpleHandler : WorkHandler
  {
    protected SimpleHandler(WorkDispatcher dispatcher, string name, int order, WorkMatch match) : base(dispatcher, name, order, match){ }
    protected SimpleHandler(WorkDispatcher dispatcher, IConfigSectionNode confNode) : base(dispatcher, confNode){ }

    protected override void DoHandleWork(WorkContext work)
    {
      work.Response.Write("{0} came".Args(5));
    }
  }
}
