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
    protected SimpleHandler(WorkHandler director, string name, int order, WorkMatch match) : base(director, name, order, match){ }
    protected SimpleHandler(WorkHandler director, IConfigSectionNode confNode) : base(director, confNode){ }

    protected override async Task DoHandleWorkAsync(WorkContext work)
    {
      await work.Response.WriteAsync("{0} came".Args(5));
    }
  }
}
