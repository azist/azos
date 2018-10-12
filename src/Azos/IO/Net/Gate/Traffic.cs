
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Azos.IO.Net.Gate
{

  public enum TrafficDirection { Incoming=0, Outgoing}

  /// <summary>
  /// Represents a traffic that passes through network gate
  /// </summary>
  public interface ITraffic
  {
    TrafficDirection Direction{get;}
    string FromAddress{get;}
    string ToAddress{get;}
    string Service{get;}
    string Method {get;}
    string RequestURL {get;}
    IDictionary<string, object> Items {get;}
  }


  /// <summary>
  /// Represents HTTP traffic that arrives via HttpListener
  /// </summary>
  public struct HTTPIncomingTraffic : ITraffic
  {
    public HTTPIncomingTraffic(HttpListenerRequest request, string realRemoteAddressHdr = null)
    {
      m_Request = request;
      m_Items = null;
      m_RealRemoteAddressHdr = realRemoteAddressHdr;
    }

    private HttpListenerRequest m_Request;
    private Dictionary<string,object> m_Items;
    private string m_RealRemoteAddressHdr;

    public TrafficDirection Direction { get{ return TrafficDirection.Incoming;}}

    public string FromAddress
    {
      get
      {
         return m_RealRemoteAddressHdr.IsNullOrWhiteSpace() ?
                      m_Request.RemoteEndPoint.Address.ToString() :
                      m_Request.Headers[m_RealRemoteAddressHdr] ?? m_Request.RemoteEndPoint.Address.ToString();
      }
    }

    public string ToAddress{ get{ return m_Request.LocalEndPoint.Address.ToString();} }

    public string Service{ get{ return m_Request.LocalEndPoint.Port.ToString();} }

    public string Method{ get{ return m_Request.HttpMethod;} }

    public string RequestURL{ get{ return m_Request.Url.ToString();}}

    public IDictionary<string, object> Items
    {
      get
      {
        if (m_Items==null)
        {
          m_Items = new Dictionary<string,object>();
          foreach(var key in m_Request.QueryString.AllKeys.Where(k=>k.IsNotNullOrWhiteSpace()))
            m_Items[key] = m_Request.QueryString.Get(key);
        }

        return m_Items;
      }
    }
  }


  /// <summary>
  /// Represents general kind of traffic not bound to any particular technology
  /// </summary>
  public struct GeneralTraffic : ITraffic
  {
    public TrafficDirection Direction { get; set;}

    public string FromAddress{ get; set; }

    public string ToAddress{ get; set; }

    public string Service{ get; set; }

    public string Method{ get; set; }

    public string RequestURL{ get; set;}

    public IDictionary<string, object> Items {get;set;}
  }


}
