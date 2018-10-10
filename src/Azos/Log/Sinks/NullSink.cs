
namespace Azos.Log.Sinks
{
  /// <summary>
  /// Log sink that does not log anything anywhere
  /// </summary>
  public class NullSink : Sink
  {

    public NullSink() { }

    public NullSink(string name) : base(name) { }

    protected internal override void DoSend(Message entry)
    {

    }

  }
}