/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace Azos.Data.Access
{

  /// <summary>
  /// Decorator interface for entities used to uniquely identify entities in a store
  /// </summary>
  public interface IDataStoreKey
  {
  }

  /// <summary>
  /// Represents a key (key field) used in databases that identify entities with BIGINT identity/autoinc columns
  /// </summary>
  [Serializable]
  public struct CounterDataStoreKey : IDataStoreKey
  {
    public readonly Int64 Counter;

    public CounterDataStoreKey(Int64 value)
    {
      Counter = value;
    }

    public override string  ToString()
    {
 	    return string.Format("KEY[COUNTER = {0}]", Counter); // do not localize
    }

    public override int  GetHashCode()
    {
 	    return Counter.GetHashCode();
    }

    public override bool  Equals(object obj)
    {
      if(obj==null || !(obj is CounterDataStoreKey)) return false;
      return Counter.Equals(((CounterDataStoreKey)obj).Counter);
    }
  }


  /// <summary>
  /// Defines dictionary of string/object pairs used for key matching, where string dictionary key represents column name
  ///  in storage and value is an object for the key
  /// </summary>
  [Serializable]
  public sealed class NameValueDataStoreKey : Dictionary<string, object>, IDataStoreKey
  {
    public NameValueDataStoreKey() : base(StringComparer.InvariantCultureIgnoreCase)
    {

    }

    public NameValueDataStoreKey(SerializationInfo info, StreamingContext context) : base(info, context)
    {

    }

    public NameValueDataStoreKey(params object[] pairs) : base(StringComparer.InvariantCultureIgnoreCase)
    {
      const string CTOR_NAME = " NameValueDataStoreKey.ctor(parms[])";

      if ((pairs.Length %2)>0)
        throw new DataAccessException(CTOR_NAME + " expects even number of args");

      for(int i=0; i<pairs.Length; i+=2)
      {
        var key = pairs[i] as string;
        var val = pairs[i+1];

        if (key==null)
          throw new DataAccessException(CTOR_NAME + " expects string as a key name");

        this.Add(key, val);
      }
    }

    public override string ToString()
    {
      var s = new StringBuilder();

      foreach(var e in this)
       s.AppendFormat("'{0}'='{1}',", e.Key, e.Value);

      if (s.Length > 0) s.Remove(s.Length - 1, 1);

      return s.ToString();
    }
  }




}
