/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

using Azos.Scripting;
using Azos.Serialization.JSON;
using Azos.CodeAnalysis.Source;
using Azos.Serialization.JSON.Backends;
using Azos.CodeAnalysis.JSON;

namespace Azos.Tests.Nub.Serialization
{
  [Runnable]
  public class JazonParserTests
  {
    [Run]
    public void ParserTest()
    {
      var json = @"{ a:       1, b: ""something"", c: null, d: {}, e: 23.7}";
      var src = new StringSource(json);
      var got = JazonParser.Parse(src, true);

      got.See();

    }
  }
}

