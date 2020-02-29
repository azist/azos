/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
      throw new NotImplementedException();
    }

    public object DeserializeFromJson(Stream stream, bool caseSensitiveMaps, Encoding encoding)
    {
      throw new NotImplementedException();
    }

    public object DeserializeFromJson(ISourceText source, bool caseSensitiveMaps)
    {
      throw new NotImplementedException();
    }
  }
}
