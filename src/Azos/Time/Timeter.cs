using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Azos.Time
{
  /// <summary>
  /// Time Meter (`Timeter` for short) - Measures time without any memory allocations.
  /// This struct is very similar to Stopwatch class and is based on its APis
  /// </summary>
  public struct Timeter
  {
    private static readonly long FREQUENCY_MS = Stopwatch.Frequency / 1000;//ms
    private static readonly double FREQUENCY_SEC = Stopwatch.Frequency;//sec


    public static Timeter StartNew()
    {
      var result = new Timeter();
      result.m_Start = Stopwatch.GetTimestamp();
      return result;
    }

    private long m_Start;
    private long m_Total;


    /// <summary> Returns true when this instance was assigned </summary>
    public bool IsStarted => m_Start > 0;


    /// <summary> Starts the measurement if it has not started yet, otherwise does nothing </summary>
    public void Start()
    {
      if (m_Start > 0) return;
      m_Start = Stopwatch.GetTimestamp();
    }

    /// <summary> Stops the time measurement if it is started, otherwise does nothing </summary>
    public void Stop()
    {
      if (m_Start == 0) return;
      m_Total = ElapsedRaw;
      m_Start = 0;
    }

    /// <summary>
    /// Returns total elapsed time that was measured so far in system ticks (not DatTime ticks)
    /// </summary>
    public long ElapsedRaw => m_Total + (IsStarted ? (Stopwatch.GetTimestamp() - m_Start) : 0);

    /// <summary>
    /// Returns elapsed interval as TimeSpan
    /// </summary>
    public TimeSpan Elapsed => TimeSpan.FromSeconds(ElapsedRaw / (double)Stopwatch.Frequency);

    /// <summary>
    /// Returns truncated number of elapsed ms. The value is truncated.
    /// This is typically used to measure Call/Api latency where sub-ms resolution is not required
    /// </summary>
    public long ElapsedMs => ElapsedRaw / FREQUENCY_MS;

    /// <summary>
    /// Returns double number of seconds
    /// </summary>
    public double ElapsedSec => ElapsedRaw / FREQUENCY_SEC;

  }
}
