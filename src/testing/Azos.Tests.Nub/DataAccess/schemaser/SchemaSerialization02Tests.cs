/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Azos.Data;
using Azos.Data.Adlib;
using Azos.Geometry;
using Azos.Scripting;
using Azos.Serialization.Bix;
using Azos.Serialization.JSON;

namespace Azos.Tests.Nub.DataAccess
{
  [Runnable]
  public class SchemaSerialization02Tests
  {
    [Bix("aaaa3966-5593-460f-bcd7-0d480310aaea")]
    [Schema(Description = "Test document A")]
    public class DocA : TypedDoc
    {
    }

    [Bix("aaaa3966-5593-460f-bcd7-0d480310aaff")]
    public class Relative : TypedDoc
    {
    }


    [Run]
    public void Case01()
    {
    }

    [Run]
    public void Case02()
    {
    }
  }
}
