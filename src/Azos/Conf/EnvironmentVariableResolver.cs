/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Collections;

namespace Azos.Conf
{

  /// <summary>
  /// Represents an entity that can resolve variables
  /// </summary>
  public interface IEnvironmentVariableResolver
  {
    /// <summary>
    /// Turns named variable into its value or null
    /// </summary>
    bool ResolveEnvironmentVariable(string name, out string value);
  }


  /// <summary>
  /// Resolves variables using Windows environment variables.
  /// NOTE: When serialized a new instance is created which will not equal by reference to static.Instance property
  /// </summary>
  [Serializable]//but there is nothing to serialize
  public sealed class OSEnvironmentVariableResolver : IEnvironmentVariableResolver
  {
    private static OSEnvironmentVariableResolver s_Instance = new OSEnvironmentVariableResolver();

    /// <summary>
    /// Returns a singleton class instance
    /// </summary>
    public static OSEnvironmentVariableResolver Instance { get { return s_Instance; } }

    private OSEnvironmentVariableResolver() { }

    public bool ResolveEnvironmentVariable(string name, out string value)
    {
      value = System.Environment.GetEnvironmentVariable(name);
      return true;
    }
  }


  public sealed class VarsDictionary : Dictionary<string, string>
  {
    public VarsDictionary() : base(StringComparer.InvariantCultureIgnoreCase) {}
    public VarsDictionary(IDictionary<string, string> other) : base(other, StringComparer.InvariantCultureIgnoreCase) { }
    private VarsDictionary(SerializationInfo info, StreamingContext context) : base(info, context) {}
  }

  /// <summary>
  /// Allows for simple ad-hoc environment var passing to configuration
  /// </summary>
  [Serializable]
  public sealed class Vars : IEnumerable<KeyValuePair<string, string>>, IEnvironmentVariableResolver
  {
    public Vars(IDictionary<string, string> initial = null)
    {
      m_Data = initial == null ? new VarsDictionary() : new VarsDictionary(initial);
    }

    [NonSerialized]
    private object m_Sync = new object();
    private volatile VarsDictionary m_Data;

    public bool ResolveEnvironmentVariable(string name, out string value)
    {
      if (m_Data.TryGetValue(name, out value)) return true;
      return false;
    }

    public string this[string name]
    {
      get
      {
        string result;
        if (m_Data.TryGetValue(name, out result)) return result;
        return null;
      }
      set
      {
        lock (m_Sync)
        {
          var data = new VarsDictionary(m_Data);

          string existing;
          if (data.TryGetValue(name, out existing)) data[name] = value;
          else data.Add(name, value);

          m_Data = data;
        }
      }
    }

    public bool Remove(string key)
    {
      lock (m_Sync)
      {
        var data = new VarsDictionary(m_Data);
        var result = data.Remove(key);
        m_Data = data;
        return result;
      }
    }

    public void Clear() { m_Data = new VarsDictionary(); }

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator() { return m_Data.GetEnumerator(); }
    IEnumerator IEnumerable.GetEnumerator() { return this.GetEnumerator(); }
  }
}
