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
