/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Log.Sinks
{
  /// <summary>
  /// Log sink that does not log anything anywhere
  /// </summary>
  public class NullSink : Sink
  {

    public NullSink(ISinkOwner owner) : base(owner)
    {

    }

    public NullSink(ISinkOwner owner, string name, int order) : base(owner, name, order)
    {
    }

    protected internal override void DoSend(Message entry)
    {
    }
  }
}