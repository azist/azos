/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Azos.Apps
{
  /// <summary>
  /// Describes call flow - an entity which represents a logical call, such as service flow.
  /// This can be used as a correlation context.
  /// Call flow exposes a `DirectorName` property which may contain a name of the primary
  /// flow originator, which can be a remote/distributed process.
  /// </summary>
  public interface ICallFlow
  {
    /// <summary>
    /// Gets a unique CallFlow identifier, you can use it for things like log correlation id
    /// </summary>
    Guid ID { get; }

    /// <summary>
    /// Returns a logical name of the primary flow originator/controller which governs the execution of various
    /// steps of the flow. Logical name may point to a distributed process/activity.
    /// Subordinate components may check this property to determine, among other things, the level of their instrumentation
    /// </summary>
    string DirectorName { get; }

    /// <summary>
    /// Sets director name explicitly.
    /// This is a logical name of the primary flow originator/controller which governs the execution of various
    /// steps of the flow. Logical name may point to a distributed process/activity.
    /// Subordinate components may check this property to determine, among other things, the level of their instrumentation
    /// </summary>
    void SetDirectorName(string name);

    /// <summary>
    /// Provides information about the remote caller address, such as a Glue node or IP address of an a remote Http client making this call
    /// </summary>
    string CallerAddress { get; }

    /// <summary>
    /// Provides short info/description about the caller application/agent/device
    /// </summary>
    string CallerAgent { get; }

    /// <summary>
    /// Describes the port/entry point through which the caller made the call, e.g. this may be set to a web Uri
    /// that initiated the call
    /// </summary>
    string CallerPort { get; }

    /// <summary>
    /// Gets/sets an ad-hoc named item values. You can use this to store arbitrary correlation values.
    /// The names use case-sensitive ordinal comparison.
    /// Note: these item values are NOT automatically sent along the call chain to the next service host.
    /// If a named item is not found, returns null
    /// </summary>
    object this[string key] { get; set; }

    /// <summary>
    /// Enumerates all items in the flow or returns an empty enumerable
    /// </summary>
    IEnumerable<KeyValuePair<string, object>> Items{ get; }
  }

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
      m_CallerAgent = callerAgent.Default(System.Reflection.Assembly.GetCallingAssembly().FullName);
      m_CallerPort = callerPort.Default(System.Reflection.Assembly.GetEntryAssembly()?.FullName);
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

  }
}
