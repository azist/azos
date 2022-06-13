/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

using Azos.Scripting;
using Azos.Data.Adlib;
using Azos.Data.Adlib.Server;

namespace Azos.Tests.Integration.MongoDb
{
  [Runnable]
  public class AdlibFilterTests
  {
    [Run]
    public void Test01()
    {
      var filter = new ItemFilter
      {

      };

      var qry = BsonConvert.GetFilterQuery(filter);

      qry.See();

    }
  }
}
