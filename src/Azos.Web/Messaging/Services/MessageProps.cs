/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Text;

using Azos.Collections;
using Azos.Serialization.JSON;

namespace Azos.Sky.Messaging.Services
{
  /// <summary>
  /// A dictionary of ad-hoc message properties.
  /// The keys are not case-sensitive. A key must be encodable as Atom - contain up to 8 ASCII characters
  /// </summary>
  public sealed class MessageProps : AdhocMapDecorator
  {
    /// <summary>
    /// Defines a limit for the length of string property value.
    /// String property values may not be longer than 255 Unicode characters in length
    /// </summary>
    public const int MAX_STRING_VALUE_LENGTH = 0xff;

    /// <summary>
    /// Denotes system primitive types mappable which MessageProps values map into
    /// </summary>
    public enum Primitive
    {
      Invalid = 0,
      String,
      Int,
      Long
    }

    /// <summary>
    /// Tries to encode a supported value to either string or long returning the resulting Primitive type.
    /// This is used for building ad hoc indexes which must be efficient.
    /// A key name is encoded as an Atom which is stored as an integer, a value is stored either as string or
    /// long which makes the index very efficient.
    /// `Primitive.Invalid` is returned when an object value is not encodable (e.g. unsupported type)
    /// </summary>
    public static (Primitive t, string str, long lng) TryEncode(object value)
    {
      if (value == null)        return (Primitive.String, null, 0);
      if (value is string str)  return str.Length <= MAX_STRING_VALUE_LENGTH ? (Primitive.String, str, 0) : (Primitive.Invalid, null, 0);
      if (value is DateTime dt) return (Primitive.Long, null, dt.ToSecondsSinceUnixEpochStart());
      if (value is int   int32) return (Primitive.Int, null, int32);
      if (value is long  int64) return (Primitive.Long, null, int64);
      if (value is bool   flag) return (Primitive.Int, null, flag ? 1 : 0);
      return (Primitive.Invalid, null, 0);
    }


    public MessageProps() : base(new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase)) { }
    public MessageProps(JsonDataMap data) : this()
     => CallGuardException.Protect(() => data.NonNull(nameof(data)).ForEach(kvp => Add(kvp)));

    protected override string CheckKey(string key)
    {
      Atom.Encode(key.NonBlankMax(sizeof(long), nameof(key)));
      return key.ToLowerInvariant();
    }

    protected override object CheckValue(object value)
      => value.IsTrue( v => TryEncode(value).t > Primitive.Invalid);

  }
}
