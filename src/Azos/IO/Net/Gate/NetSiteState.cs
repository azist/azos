
using System;
using System.Collections.Generic;

namespace Azos.IO.Net.Gate
{
  /// <summary>
  /// Represents the state of the metwrk site - it can be a particular address or group
  /// </summary>
  public sealed class NetSiteState
  {
      internal NetSiteState(Group group)
      {
        Group = group;
        m_LastTouch = DateTime.UtcNow;
      }

      internal NetSiteState(string addr)
      {
        Address = addr;
        m_LastTouch = DateTime.UtcNow;
      }


      internal Dictionary<string, _value> m_Variables = new Dictionary<string, _value>(StringComparer.OrdinalIgnoreCase);
      internal DateTime m_LastTouch;



      public readonly string Address;
      public readonly Group Group;

      public string Key { get{ return Group==null ? Address : Group.Key;}}


        internal class _value
        {
          public long Value;
        }

  }


}
