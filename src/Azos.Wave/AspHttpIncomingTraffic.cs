/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

using Azos.IO.Net.Gate;

namespace Azos.Wave
{
  /// <summary>
  /// Represents HTTP traffic that arrives via ASP http context
  /// </summary>
  public sealed class AspHttpIncomingTraffic : ITraffic
  {
    public const string NO_IP = "0.0.0.0";

    public AspHttpIncomingTraffic(HttpContext httpContext, string realRemoteAddressHdr = null)
    {
      m_HttpContext = httpContext;
      m_Items = null;
      m_RealRemoteAddressHdr = realRemoteAddressHdr;
    }

    private HttpContext m_HttpContext;
    private Dictionary<string, object> m_Items;
    private string m_RealRemoteAddressHdr;

    public TrafficDirection Direction { get { return TrafficDirection.Incoming; } }

    public string FromAddress
    {
      get
      {
        if (m_RealRemoteAddressHdr.IsNullOrWhiteSpace())
        {
          return (m_HttpContext.Connection.RemoteIpAddress?.ToString()).Default(NO_IP);
        }

        string rIP = m_HttpContext.Request.Headers[m_RealRemoteAddressHdr];

        if (m_RealRemoteAddressHdr.EqualsOrdIgnoreCase(WebConsts.HTTP_HDR_X_FORWARDED_FOR) && rIP.IsNotNullOrWhiteSpace())
        {
          var ic = rIP.IndexOf(',');
          if (ic >= 0 && ic < rIP.Length - 1)
          {
            return rIP = rIP.Substring(ic);//take the first client IP address in the header list to prevent spoofing
            // see: https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Forwarded-For
          }
        }

        return (rIP ?? m_HttpContext.Connection.RemoteIpAddress?.ToString()).Default(NO_IP);
      }
    }

    public string ToAddress => (m_HttpContext.Connection.LocalIpAddress?.ToString()).Default(NO_IP);

    public string Service => m_HttpContext.Connection?.LocalPort.ToString();

    public string Method => m_HttpContext.Request.Method;

    public string RequestURL => m_HttpContext.Request.ToEncodedUrl();

    public IDictionary<string, object> Items
    {
      get
      {
        if (m_Items == null)
        {
          m_Items = new Dictionary<string, object>();

          foreach(var pair in m_HttpContext.Request.Query)
          {
            if (pair.Key.IsNotNullOrWhiteSpace())
            {
              m_Items[pair.Key] = pair.Value.ToString();
            }
          }
        }
        return m_Items;
      }
    }
  }


}
