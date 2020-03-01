/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Azos.Scripting;
using Azos.Time;
using Azos.Serialization.JSON;
using Azos.CodeAnalysis.Source;
using Azos.Serialization.JSON.Backends;
using System.Linq;

namespace Azos.Tests.Nub.Serialization
{
  [Runnable]
  public class JazonLexerTests
  {
    static JazonLexerTests()
    {
      JsonReader.____SetReaderBackend(new JazonReaderBackend());
    }

    [Run]
    public void LexerTest()
    {
      var json = @"{ a:       1, b: ""something"", c: null, d: {}, e: 23.7}";
      var src = new StringSource(json);
      var lxr = new JazonLexer(src);

      lxr.Select(t => "{0} `{1}`".Args(t.Type, t.Text) ).See();

    }

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

