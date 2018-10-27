
namespace Azos.Erlang
{
  public enum Direction
  {
    Inbound,
    Outbound
  }

  public enum ErlTraceLevel
  {
    /// <summary>
    /// Tracing is off
    /// </summary>
    Off         = 0,

    /// <summary>
    /// Trace ordinary send and receive messages
    /// </summary>
    Send        = 1,

    /// <summary>
    /// Trace control messages (e.g. link/unlink)
    /// </summary>
    Ctrl        = 2,

    /// <summary>
    /// Trace handshaking at connection startup
    /// </summary>
    Handshake   = 3,

    /// <summary>
    /// Trace Epmd connectivity
    /// </summary>
    Epmd        = 4,

    /// <summary>
    /// Trace wire-level message content
    /// </summary>
    Wire        = 5
  }

  /// <summary>
  /// Debugging delegate called to be able to record transport-related events
  /// </summary>
  /// <param name="sender">Event sender</param>
  /// <param name="type">Type of trace event</param>
  /// <param name="dir">Event direction (in/out-bound)</param>
  /// <param name="message">Event detail</param>
  public delegate void TraceEventHandler(object sender, ErlTraceLevel type, Direction dir, string message);
}