/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;


namespace Azos.Sky.Metabase{ public sealed partial class Metabank{

  /// <summary>
  /// Kepos information during validation scan
  /// </summary>
  public class ValidationContext
  {

    public ValidationContext(IList<MetabaseValidationMsg> output)
    {
       Output = output;
       State = new Dictionary<string, object>( StringComparer.InvariantCultureIgnoreCase );
    }


    /// <summary>
    /// The output target (i.e. EventedList)
    /// </summary>
    public readonly IList<MetabaseValidationMsg> Output;

    /// <summary>
    /// Dictionary of variables that may be needed to retain state during validation
    /// </summary>
    public readonly Dictionary<string, object> State;

    public T StateAs<T>(string key) where T : class
    {
      object value;
      if (State.TryGetValue(key, out value))
        return value as T;

      return null;
    }
  }



}}
