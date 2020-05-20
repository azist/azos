using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.Standards.Medical
{
  /// <summary>
  /// United States - National Drug Code
  /// </summary>
  [Serializable]
  public struct UsNdc : IEquatable<UsNdc>, IRequired, IJsonReadable, IJsonWritable, IFormattable
  {
    public ulong Value;

    public bool IsAssigned => Value != 0;

    public bool CheckRequired(string targetName) => IsAssigned;

    public override string ToString() => ToString(null, null);

    public string ToString(string format, IFormatProvider formatProvider)
    {
      throw new NotImplementedException();
    }

    public override int GetHashCode() => (int)(Value >> 32) ^ (int)Value;

    public override bool Equals(object obj) => obj is UsNdc other ? this.Equals(other) : false;

    public bool Equals(UsNdc other) => Value == other.Value;

    public static bool operator ==(UsNdc a, UsNdc b) => a.Equals(b);
    public static bool operator !=(UsNdc a, UsNdc b) => !a.Equals(b);

    public (bool match, IJsonReadable self) ReadAsJson(object data, bool fromUI, JsonReader.DocReadOptions? options)
    {
      throw new NotImplementedException();
    }

    public void WriteAsJson(TextWriter wri, int nestingLevel, JsonWritingOptions options = null)
    {
      throw new NotImplementedException();
    }

    public static UsNdc Parse(string value)
    {
      throw new NotImplementedException();
    }

    public static bool TryParse(string str, out UsNdc value)
    {
      throw new NotImplementedException();
    }

  }
}
