/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

using Azos.Data.Business;
using Azos.Serialization.Bix;
using Azos.Serialization.JSON;

namespace Azos.Data.Access.Rpc
{
  /// <summary>
  /// Represents a command sent to proxy for execution
  /// </summary>
  [Bix("a7c8277b-cd00-44f6-b154-607f34f29ad8")]
  [Schema(Description = "Represents a command sent to proxy for execution as a part of request")]
  public sealed class Command : FragmentModel
  {
    [Bix("507ad8fa-8ab9-4ce8-ab4d-482e9b2a0d18")]
    [Schema(Description = "Represents a command parameter which has a `Name`, `Value`, and `TypeHint`")]
    public sealed class Param : FragmentModel
    {
      /// <summary>
      /// Provides typing hints for data transmission, e.g. an otherwise-integer value `5` may need to be treated as `Double`
      /// </summary>
      public enum TypeHint
      {
        Unspecified = 0,
        Int,
        Long,
        Date,
        Double,
        Decimal,
        Bool,
        String,
        Blob,
        OutResult
      }

      /// <summary>
      /// Tries to map a TypeHint into a CLR type or null if no mapping possible
      /// </summary>
      public static Type TryMap(TypeHint hint) => s_Map.TryGetValue(hint, out var result) ? result : null;
      private static readonly Dictionary<TypeHint, Type> s_Map = new Dictionary<TypeHint, Type>
      {
         { TypeHint.Int     , typeof(int) },
         { TypeHint.Long    , typeof(long) },
         { TypeHint.Date    , typeof(DateTime) },
         { TypeHint.Double  , typeof(double) },
         { TypeHint.Decimal , typeof(decimal) },
         { TypeHint.Bool    , typeof(bool) },
         { TypeHint.String  , typeof(string) },
         { TypeHint.Blob    , typeof(byte[]) }
      };


      public Param() { }

      public Param(string name, object value, TypeHint hint = TypeHint.Unspecified)
      {
        Name = name.NonBlank(nameof(name));
        Value = value;
        Type = hint;
      }

      /// <summary>
      /// Name of the command parameter, typically as used in text such as SQL parameter name
      /// </summary>
      [Field(required: true, Description = "Name of the command parameter, typically as used in text such as SQL parameter name")]
      public string Name { get; set; }

      /// <summary>
      /// Parameter value or null. The value would be tried to type-cast using a `TypeHint`
      /// </summary>
      [Field(Description = "Parameter value or null. The value would be tried to type-cast using a `TypeHint`")]
      public object Value { get; set; }

      /// <summary>
      /// Provides typing hints for data transmission, e.g. an otherwise-integer value `5` may need to be treated as `Double`
      /// </summary>
      [Field(Description = "Provides typing hints for data transmission, e.g. an otherwise-integer value `5` may need to be treated as `Double`")]
      public TypeHint Type { get; set; }
    }

    /// <summary>
    /// Command text, such as a SQL statement containing full text or command name
    /// </summary>
    [Field(required: true, description: "Command text, such as a SQL statement containing full text or command name")]
    public string Text { get; set; }

    /// <summary>
    /// Provides optional header map used for command execution, e.g. some provider may require to specify additional routing information
    /// </summary>
    [Field(description: "Provides optional header map used for command execution, e.g. some provider may require to specify additional routing information")]
    public JsonDataMap Headers { get; set; }

    /// <summary>
    /// Optional parameter collection to pass to command. Some commands do not require any parameters
    /// </summary>
    [Field(description: "Optional parameter collection to pass to command. Some commands do not require any parameters")]
    public List<Param> Parameters { get; set; }
  }
}
