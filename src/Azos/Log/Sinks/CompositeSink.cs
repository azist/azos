/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Conf;
using Azos.Collections;

namespace Azos.Log.Sinks
{
  /// <summary>
  /// Provides an abstraction of a wrap around another destinations
  /// </summary>
  public class CompositeSink : Sink, ISinkOwnerRegistration
  {

    public CompositeSink(ISinkOwner owner) : base(owner)
    {
    }

    public CompositeSink(ISinkOwner owner, string name, int order) : base(owner, name, order)
    {
    }

    protected override void Destructor()
    {
      base.Destructor();

      foreach (var sink in m_Sinks.OrderedValues.Reverse())
        sink.Dispose();
    }

    private OrderedRegistry<Sink> m_Sinks = new OrderedRegistry<Sink>();


    LogDaemonBase ISinkOwner.LogDaemon => ComponentDirector.LogDaemon;
    void ISinkOwnerRegistration.Register(Sink sink) => m_Sinks.Register(sink);
    void ISinkOwnerRegistration.Unregister(Sink sink) => m_Sinks.Unregister(sink);

    /// <summary> Returns sinks that this sink wraps. This call is thread safe </summary>
    public IEnumerable<Sink> Sinks => m_Sinks.OrderedValues;


    protected override void DoConfigure(Conf.IConfigSectionNode node)
    {
      base.DoConfigure(node);

      foreach (var dnode in node.Children.Where(n => n.Name.EqualsIgnoreCase(LogDaemonBase.CONFIG_SINK_SECTION)))
      {
        var sink = FactoryUtils.MakeAndConfigure<Sink>(dnode, typeof(CSVFileSink), new[] { this });
      }
    }

    protected override void DoStart()
    {
      base.DoStart();

      foreach (var sink in m_Sinks.OrderedValues)
        try
        {
          sink.Start();
        }
        catch (Exception error)
        {
          throw new AzosException(
                StringConsts.LOGDAEMON_SINK_START_ERROR.Args(Name, sink.Name, sink.TestOnStart, error.Message),
                error);
        }
    }

    protected override void DoSignalStop()
    {
      base.DoSignalStop();
      //Attention!!! It is important here NOT TO NOTIFY sinks of pending shutdown,
      //so that LogDaemon may start terminating all by itself and it commits all messages to sinks
      //that should be still operational.
    }

    protected override void DoWaitForCompleteStop()
    {
      base.DoWaitForCompleteStop();
      // at this point the thread has stopped and we can now stop the sinks

      foreach (var sink in m_Sinks.OrderedValues.Reverse())
      {
        try
        {
          sink.WaitForCompleteStop();
        }
        catch
        {
#warning REVISE - must not eat exceptions
        }  // Can't do much here in case of an error
      }
    }

    protected internal override void DoSend(Message entry) => m_Sinks.OrderedValues.ForEach( s => s.Send(entry) );
    protected internal override void DoPulse()             => m_Sinks.OrderedValues.ForEach(s => s.Pulse() );
  }
}
