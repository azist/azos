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
    public object DeserializeFromJson(string json, bool caseSensitiveMaps)
    {
      var source = new StringSource(json, JsonLanguage.Instance);//todo: reuse instance
      return JazonParser.Parse(source, caseSensitiveMaps);
    }

    public object DeserializeFromJson(Stream stream, bool caseSensitiveMaps, Encoding encoding)
    {
      using (var source = encoding == null ? new StreamSource(stream, JsonLanguage.Instance)
                                           : new StreamSource(stream, encoding, JsonLanguage.Instance))
        return JazonParser.Parse(source, caseSensitiveMaps);
    }

    public object DeserializeFromJson(ISourceText source, bool caseSensitiveMaps)
    {
      return JazonParser.Parse(source, caseSensitiveMaps);
    }

    public Task<object> DeserializeFromJsonAsync(Stream stream, bool caseSensitiveMaps, Encoding encoding)
    {
#warning AZ #731 rewrite async deserializer core
      return Task.FromResult(DeserializeFromJson(stream, caseSensitiveMaps, encoding));
    }

    public Task<object> DeserializeFromJsonAsync(ISourceText source, bool caseSensitiveMaps)
    {
#warning AZ #731 rewrite async deserializer core
      return Task.FromResult(DeserializeFromJson(source, caseSensitiveMaps));
    }
  }
}
