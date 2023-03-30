/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Azos.CodeAnalysis.JSON;
using Azos.CodeAnalysis.Source;

namespace Azos.Serialization.JSON.Backends
{
  /// <summary>
  /// Implements default [Az]os [J]s[on] parser which uses allocation-free token processing
  /// </summary>
  public sealed class JazonReaderBackend : IJsonReaderBackend
  {
    public object DeserializeFromJson(string json, JsonReadingOptions ropt)
    {
      var source = new StringSource(json, JsonLanguage.Instance);
      return JazonParser.Parse(source, ropt);
    }

    public object DeserializeFromJson(Stream stream, Encoding encoding, bool useBom, JsonReadingOptions ropt)
    {
      using (var source = new StreamSource(stream, encoding, useBom, JsonLanguage.Instance))
      {
        return JazonParser.Parse(source, ropt);
      }
    }

    public object DeserializeFromJson(ISourceText source, JsonReadingOptions ropt)
     => JazonParser.Parse(source, ropt);

    public async ValueTask<object> DeserializeFromJsonAsync(Stream stream, Encoding encoding, bool useBom, JsonReadingOptions ropt)
    {
      using (var source = new StreamSource(stream, encoding, useBom, JsonLanguage.Instance))
      {
        return await JazonParserAsync.ParseAsync(source, ropt).ConfigureAwait(false);
      }
    }

    public ValueTask<object> DeserializeFromJsonAsync(ISourceText source, JsonReadingOptions ropt)
     => JazonParserAsync.ParseAsync(source,  ropt);
  }
}
