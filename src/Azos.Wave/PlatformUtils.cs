
using System.Net;

namespace Azos.Wave
{
  public static class PlatformUtils
  {

    //todo: Rewrite with new WAVE server!!!
    public static void SetRequestQueueLimit(HttpListener listener, long len){}



    /*
    /// <summary>
    /// Must be called after Listener.Start();
    /// </summary>
    public unsafe static void SetRequestQueueLimit(HttpListener listener, long len)
    {
      if (OS.Computer.IsMono)
        return;

      var prop_RequestQueueHandle = typeof(HttpListener).GetProperties(BindingFlags.NonPublic | BindingFlags.Instance)
                                                        .First(p => p.Name.Equals("RequestQueueHandle"));

      var requestQueueHandle = (CriticalHandle)prop_RequestQueueHandle.GetValue(listener, null);
      var result = HttpSetRequestQueueProperty(requestQueueHandle,
                                               HTTP_SERVER_PROPERTY.HttpServerQueueLengthProperty,
                                               new IntPtr((void*)&len),
                                               (uint)Marshal.SizeOf(len),
                                               0,
                                               IntPtr.Zero);

      if (result != 0)
        throw new HttpListenerException((int)result);
    }

    [DllImport("httpapi.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
    internal static extern uint HttpSetRequestQueueProperty(
        CriticalHandle requestQueueHandle,
        HTTP_SERVER_PROPERTY serverProperty,
        IntPtr pPropertyInfo,
        uint propertyInfoLength,
        uint reserved,
        IntPtr pReserved);

    internal enum HTTP_SERVER_PROPERTY
    {
      HttpServerAuthenticationProperty,
      HttpServerLoggingProperty,
      HttpServerQosProperty,
      HttpServerTimeoutsProperty,
      HttpServerQueueLengthProperty,
      HttpServerStateProperty,
      HttpServer503VerbosityProperty,
      HttpServerBindingProperty,
      HttpServerExtendedAuthenticationProperty,
      HttpServerListenEndpointProperty,
      HttpServerChannelBindProperty,
      HttpServerProtectionLevelProperty,
    }

    */
  }
}