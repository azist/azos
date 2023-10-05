/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Azos.Serialization.JSON;

namespace Azos.Apps
{
  /// <summary>
  /// Implements ICallFlow interface for flows initiated by calling code.
  /// For example, a console app may be considered an initiator of a `ICallFlow` by allocating an instance of `CodeCallFlow`.
  /// Contrast this with a typical Web API app which has a flow initiated by a web call (e.g. via Wave WorkContext)
  /// </summary>
  public sealed class CodeCallFlow : ICallFlow
  {
    public const string CODE_CALLER_ADDRESS = "code";

    /// <summary>
    /// Initiates a call flow from calling code
    /// </summary>
    /// <param name="guid">The flow Guid may come from a remote party, hence it needs to be passed-in</param>
    /// <param name="directorName">An optional director name which can come from a remote party</param>
    /// <param name="callerAgent">An optional caller agent, if not passed-in then the system will use the CallingAssembly FQN</param>
    /// <param name="callerPort">An optional caller port (point of entry), if not passed-in then the system will use the EntryAssembly FQN</param>
    public CodeCallFlow(Guid guid, string directorName = null, string callerAgent = null, string callerPort = null)
    {
      m_Id = guid;
      m_DirectorName = directorName;
      m_CallerAgent = callerAgent.Default(System.Reflection.Assembly.GetCallingAssembly().GetName().Name);
      m_CallerPort = callerPort.Default(System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name);
    }

    private Guid m_Id;
    private string m_DirectorName;
    private string m_CallerAgent;
    private string m_CallerPort;
    private object m_Lock = new object();
    private volatile ConcurrentDictionary<string, object> m_Items;

    public Guid ID => m_Id;
    public string DirectorName => m_DirectorName;
    public void SetDirectorName(string name) => m_DirectorName = name;

    public string CallerAddress => CODE_CALLER_ADDRESS;
    public string CallerAgent => m_CallerAgent;
    public string CallerPort => m_CallerPort;

    public IEnumerable<KeyValuePair<string, object>> Items => m_Items != null ? m_Items : Enumerable.Empty<KeyValuePair<string, object>>();

    public object this[string key]
    {
      get
      {
        key.NonNull(nameof(key));
        if (m_Items==null) return null;
        if (m_Items.TryGetValue(key, out var result)) return result;
        return null;
      }
      set
      {
        key.NonNull(nameof(key));
        if (m_Items == null)
        {
          lock(m_Lock)
          {
            if (m_Items == null)
            {
              m_Items = new ConcurrentDictionary<string, object>();
            }
          }
        }
        m_Items[key] = value;
      }//set
    }

    //20230907 DKh #893
    public JsonDataMap RepresentAsJson()
     => new JsonDataMap
     {
       {"tp", "code"},
       {nameof(ICallFlow.ID), ID },
       {nameof(ICallFlow.DirectorName), DirectorName },
       {nameof(ICallFlow.CallerAddress), CallerAddress },
       {nameof(ICallFlow.CallerAgent), CallerAgent },
       {nameof(ICallFlow.CallerPort), CallerPort },
       {nameof(ICallFlow.Items), Items?.ToArray() },
     };


  }
}
