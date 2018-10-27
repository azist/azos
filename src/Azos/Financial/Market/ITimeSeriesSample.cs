/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

namespace Azos.Financial.Market
{
  /// <summary>
  /// Represents an interface to an object that has a timestamp
  /// </summary>
  public interface ITimedSample
  {
    /// <summary>
    /// Timestamp of the sample
    /// </summary>
    DateTime TimeStamp { get; set; }
  }

  /// <summary>
  /// Represents a sample of a TimeSeries stream
  /// </summary>
  public interface ITimeSeriesSample : ITimedSample
  {
    /// <summary>
    /// Associates an arbitrary data
    /// </summary>
    object AssociatedData { get;}

    /// <summary>
    /// Makes aggregate instance
    /// </summary>
    ITimeSeriesSample MakeAggregateInstance();

    /// <summary>
    /// Adds a sample to this aggregation instance
    /// </summary>
    void AggregateSample(ITimeSeriesSample sample);

    /// <summary>
    /// Summarizes aggregation on this isntance
    /// </summary>
    void SummarizeAggregation();
  }


  /// <summary>
  /// Represents a sample of a TimeSeries stream
  /// </summary>
  public abstract class TimeSeriesSampleBase : ITimeSeriesSample
  {
    protected TimeSeriesSampleBase(DateTime timeStamp)
    {
      m_TimeStamp = timeStamp;
    }

    private DateTime m_TimeStamp;

    public DateTime TimeStamp{ get { return m_TimeStamp;} set { m_TimeStamp = value; } }

    /// <summary>
    /// Associates an arbitrary data
    /// </summary>
    public object AssociatedData { get; set;}


    public virtual ITimeSeriesSample MakeAggregateInstance()
    {
      throw new NotImplementedException(GetType().FullName+".MakeAggregateInstance()");
    }

    public virtual void AggregateSample(ITimeSeriesSample sample)
    {
      throw new NotImplementedException(GetType().FullName+".AggregateSample(sample)");
    }

    public virtual void SummarizeAggregation() { }
  }


}
